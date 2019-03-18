using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Rift.Configuration;
using Rift.Data.Models;
using Rift.Data.Models.Cooldowns;
using Rift.Data.Models.LolData;
using Rift.Embeds;
using Rift.Services.Riot;

using IonicLib;
using IonicLib.Extensions;
using IonicLib.Util;

using Newtonsoft.Json;

using Discord;
using Discord.WebSocket;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

using MingweiSamuel.Camille;
using MingweiSamuel.Camille.Enums;
using MingweiSamuel.Camille.LeagueV4;
using MingweiSamuel.Camille.MatchV4;
using MingweiSamuel.Camille.SummonerV4;

namespace Rift.Services
{
    public class RiotService
    {
        static Champion championData;
        static Dictionary<string, ChampionData> champions = new Dictionary<string, ChampionData>();

        static readonly string DataDragonDataFolder = Path.Combine(RiftBot.AppPath, "lol-data");
        static readonly string TempFolder = Path.Combine(DataDragonDataFolder, "temp");

        const string DataDragonVersionsUrl = "https://ddragon.leagueoflegends.com/api/versions.json";
        const string DataDragonTarballUrlTemplate = "https://ddragon.leagueoflegends.com/cdn/dragontail-{0}.tgz";
        const string DataDragonChampPortraitTemplate = "https://ddragon.leagueoflegends.com/cdn/{0}/img/champion/{1}";

        public delegate void RankChanged(RankChangedEventArgs e);

        public event RankChanged OnRankChanged;

        static uint index = 0;
        static UserLastLolAccountUpdateTime[] users = null;
        static Timer updateTimer;

        static Timer approveTimer;
        static readonly TimeSpan approveCheckCooldown = TimeSpan.FromMinutes(1);

        static readonly SemaphoreSlim registerMutex = new SemaphoreSlim(1);

        static RiotApi api;

        public RiotService()
        {
            if (!Directory.Exists(DataDragonDataFolder))
                Directory.CreateDirectory(DataDragonDataFolder);

            if (Directory.Exists(TempFolder))
            {
                Directory.Delete(TempFolder, true);
                Directory.CreateDirectory(TempFolder);
            }
            else
            {
                Directory.CreateDirectory(TempFolder);
            }

            api = RiotApi.NewInstance(Settings.App.RiotApiKey);

            approveTimer = new Timer(async delegate { await CheckApproveAsync(); }, null, TimeSpan.FromSeconds(20),
                                     approveCheckCooldown);
            updateTimer = new Timer(async delegate { await UpdateUsersAsync(); }, null, TimeSpan.FromSeconds(20),
                                    TimeSpan.FromMinutes(5));
        }

        #region Data

        public async Task InitAsync()
        {
            await UpdateDataAsync();
        }

        static async Task UpdateDataAsync()
        {
            (bool newDataAvailable, string version) = await CheckNewVersionAsync().ConfigureAwait(false);

            if (!newDataAvailable)
            {
                LoadData();

                return;
            }

            RiftBot.Log.Info($"Initiating data update: {Settings.App.LolVersion} -> {version}");

            await DownloadDataAsync(version).ContinueWith(async x =>
            {
                if (x.IsFaulted)
                {
                    RiftBot.Log.Error(x.Exception, "Data download failed!");
                    RiftBot.Log.Warn( "Falling back to existing data.");
                    LoadData();

                    return;
                }

                string path = Path.Combine(TempFolder, $"{version}.tgz");

                await UnpackData(path, version).ContinueWith(async y =>
                {
                    if (!y.IsCompletedSuccessfully)
                    {
                        RiftBot.Log.Error(y.Exception, "Data download failed!");
                        LoadData();

                        return;
                    }

                    string tempPath = Path.Combine(TempFolder, version, "champion.json");
                    string realPath = Path.Combine(DataDragonDataFolder, "champion.json");

                    try
                    {
                        if (File.Exists(realPath))
                            File.Delete(realPath);

                        File.Move(tempPath, realPath);
                    }
                    catch (Exception ex)
                    {
                        RiftBot.Log.Error(ex);
                        return;
                    }

                    LoadData();

                    Settings.App.LolVersion = version;
                    await Settings.Save(SettingsType.App);

                    RiftBot.Log.Info($"Data update completed. New version is {Settings.App.LolVersion}");

                    await RiftBot.SendMessageToDevelopers($"Data update completed."
                                                          + $" New version is {Settings.App.LolVersion}.");
                });
            });
        }

        static async Task<(bool, string)> CheckNewVersionAsync()
        {
            var response = await GetAsync(DataDragonVersionsUrl);

            if (response.StatusCode != 200)
            {
                RiftBot.Log.Error($"Failed to execute GET request: {response.StatusCode.ToString()}");

                return (false, string.Empty);
            }

            string version = JsonConvert.DeserializeObject<List<string>>(response.Content).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(version))
            {
                RiftBot.Log.Warn($"Failed to parse data version!");

                return (false, string.Empty);
            }

            if (version.Equals(Settings.App.LolVersion, StringComparison.InvariantCultureIgnoreCase))
            {
                RiftBot.Log.Info($"Data version {Settings.App.LolVersion} is up to date.");

                return (false, string.Empty);
            }

            RiftBot.Log.Info($"Found new version: {version}");

            return (true, version);
        }

        static async Task DownloadDataAsync(string version)
        {
            using (var client = new WebClient())
            {
                string file = Path.Combine(TempFolder, $"{version}.tgz");
            
                if (File.Exists(file))
                    return;
            
                await client.DownloadFileTaskAsync(new Uri(string.Format(DataDragonTarballUrlTemplate, version)), file);
            }
        }

        static void LoadData()
        {
            string json = File.ReadAllText(Path.Combine(DataDragonDataFolder, "champion.json"));
            championData = JsonConvert.DeserializeObject<Champion>(json);

            champions.Clear();

            foreach (var champData in championData.Data)
            {
                champions.Add(champData.Value.Key, champData.Value);
            }

            RiftBot.Log.Info($"Loaded {champions.Count.ToString()} champion(s).");
        }

        static async Task UnpackData(string pathToArchive, string version)
        {
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(pathToArchive);
            string tarFile = Path.Combine(TempFolder, nameWithoutExtension + ".tar");
            string tempVersionFolder = Path.Combine(TempFolder, version);

            await Task.Run(() =>
            {
                try
                {
                    if (!File.Exists(tarFile))
                    {
                        byte[] buffer = new byte[4096];

                        using (var gzip = new FileStream(pathToArchive, FileMode.Open, FileAccess.Read))
                        using (var gzipStream = new GZipInputStream(gzip))
                        using (var gzipOut = File.Create(tarFile))
                        {
                            StreamUtils.Copy(gzipStream, gzipOut, buffer);
                        }
                    }

                    using (var tar = new FileStream(tarFile, FileMode.Open, FileAccess.Read))
                    using (var tarIn = new TarInputStream(tar))
                    {
                        TarEntry entry;

                        while ((entry = tarIn.GetNextEntry()) != null)
                        {
                            if (entry.IsDirectory)
                                continue;

                            if (!entry.Name.Contains($"ru_RU/champion.json"))
                                continue;

                            string name = entry.Name.Replace('/', Path.DirectorySeparatorChar);

                            if (Path.IsPathRooted(name))
                                name = name.Substring(Path.GetPathRoot(name).Length);

                            Directory.CreateDirectory(tempVersionFolder);

                            using (var tarOut = new FileStream(Path.Combine(tempVersionFolder, "champion.json"), FileMode.Create))
                            {
                                tarIn.CopyEntryContents(tarOut);
                            }
                            
                            break;
                        }
                    }

                    File.Delete(pathToArchive);
                    File.Delete(tarFile);
                }
                catch (Exception ex)
                {
                    RiftBot.Log.Error(ex, "Failed to unpack downloaded data");

                    return;
                }
            });
        }

        public ChampionData GetChampionById(string id)
        {
            var champion = champions.Values.FirstOrDefault(x => x.Key.Equals(id, StringComparison.InvariantCultureIgnoreCase));

            if (champion is null)
                RiftBot.Log.Error($"Champion id \"{id}\" does not exist.");

            return champion;
        }

        #endregion Data

        #region Validation

        public async Task<Embed> RegisterAsync(ulong userId, string region, string summonerName)
        {
            await registerMutex.WaitAsync().ConfigureAwait(false);

            try
            {
                return await RegisterInternalAsync(userId, region, summonerName);
            }
            finally
            {
                registerMutex.Release();
            }
        }

        async Task<Embed> RegisterInternalAsync(ulong userId, string region, string summonerName)
        {
            if (await Database.HasLolDataAsync(userId))
                return RegisterEmbeds.AlreadyHasAcc;

            if (await IsPending(userId))
                return RegisterEmbeds.AcceptPending;

            (var summonerResult, var summoner) = await GetSummonerByNameAsync(region.ToLowerInvariant(), summonerName);

            if (summonerResult != RequestResult.Success)
                return RegisterEmbeds.WrongSummonerName(summonerName, region);

            if (await Database.IsTakenAsync(region, summoner.Id))
                return RegisterEmbeds.AccountIsUsed;

            string code = Helper.GenerateConfirmationCode(16);

            var pendingUser = new RiftPendingUser()
            {
                UserId = userId,
                Region = region.ToLowerInvariant(),
                PlayerUUID = summoner.Puuid,
                AccountId = summoner.AccountId,
                SummonedId = summoner.Id,
                ConfirmationCode = code,
                ExpirationTime = DateTime.UtcNow + Settings.Economy.PendingUserLifeTime
            };

            if (!await AddForApprovalAsync(pendingUser))
                return GenericEmbeds.Error;

            return RegisterEmbeds.CodeGenerated(code);
        }

        static async Task<bool> AddForApprovalAsync(RiftPendingUser pendingUser)
        {
            if (await Database.IsPendingAsync(pendingUser.UserId))
                return false;

            await Database.AddPendingUserAsync(pendingUser);
            return true;
        }

        async Task CheckApproveAsync()
        {
            var pendingUsers = await Database.GetAllPendingUsersAsync();

            if (pendingUsers is null || !pendingUsers.Any())
                return;

            RiftBot.Log.Debug($"Checking {pendingUsers.Count} pending users...");

            foreach (var user in pendingUsers)
            {
                bool expired = DateTime.UtcNow > user.ExpirationTime;

                if (expired)
                {
                    await Database.RemovePendingUserAsync(user);
                }
            }

            int usersValidated = 0;

            foreach (var user in pendingUsers)
            {
                RequestResult codeResult;
                string code;
                try
                {
                    (codeResult, code) = await GetThirdPartyCodeByEncryptedSummonerIdAsync(user.Region, user.SummonedId);
                }
                catch (Exception)
                {
                    continue;
                }

                if (code is null || codeResult != RequestResult.Success)
                    continue;

                string sanitizedCode = new string(code.Where(x => Char.IsLetterOrDigit(x)).ToArray());

                if (sanitizedCode != user.ConfirmationCode)
                    continue;

                (RequestResult requestResult, Summoner summoner) =
                    await GetSummonerByEncryptedUUIDAsync(user.Region, user.PlayerUUID);

                if (requestResult != RequestResult.Success)
                {
                    await Database.RemovePendingUserAsync(user);
                    continue;
                }

                var lolData = new RiftLolData
                {
                    UserId = user.UserId,
                    SummonerRegion = user.Region,
                    PlayerUUID = user.PlayerUUID,
                    AccountId = user.AccountId,
                    SummonerId = user.SummonedId,
                    SummonerName = summoner.Name
                };

                await Database.AddLolDataAsync(lolData);

                usersValidated++;

                await PostValidateAsync(user);
            }

            RiftBot.Log.Info($"{usersValidated} users validated.");
        }

        async Task PostValidateAsync(PendingUser pendingUser)
        {
            var sgUser = IonicClient.GetGuildUserById(Settings.App.MainGuildId, pendingUser.UserId);

            if (sgUser is null)
                return;

            await Database.RemovePendingUserAsync(pendingUser.UserId);

            await AssignRoleFromRankAsync(sgUser, pendingUser);

            await Database.AddInventoryAsync(sgUser.Id, chests: 2u);

            await sgUser.SendEmbedAsync(RegisterEmbeds.RegistrationSuccessful);

            await sgUser.SendEmbedAsync(RegisterEmbeds.RegistrationReward);

            if (!IonicClient.GetTextChannel(Settings.App.MainGuildId, Settings.ChannelId.Chat, out var channel))
                return;

            await channel.SendEmbedAsync(RegisterEmbeds.ChatEmbed(sgUser));
        }

        async Task<bool> IsPending(ulong userId)
        {
            return await Database.IsPendingAsync(userId);
        }

        #endregion Validation

        #region Rank

        async Task UpdateUsersAsync()
        {
            if (users == null || index == users.Length)
            {
                users = await Database.GetTenUsersForUpdateAsync();
                index = 0;
                return;
            }

            var user = users[index];
            if (user.LastUpdateTime + Settings.Economy.LolAccountUpdateCooldown > DateTime.UtcNow)
                return;

            try
            {
                var sgUser = IonicClient.GetGuildUserById(Settings.App.MainGuildId, user.UserId);

                if (sgUser is null)
                    await Database.RemoveLolDataAsync(user.UserId);
                else
                    await UpdateRankAsync(user.UserId, true);
            }
            catch (Discord.Net.HttpException)
            {
            }

            await Database.SetLastLolAccountUpdateTimeAsync(user.UserId, DateTime.UtcNow);
            index++;
        }

        public async Task UpdateRankAsync(ulong userId, bool silentMode = false)
        {
            if (!IonicClient.GetTextChannel(Settings.App.MainGuildId, Settings.ChannelId.Chat, out var chatChannel))
                return;

            RiftBot.Log.Debug($"[User|{userId}] Getting summoner for rank update");

            var lolData = await Database.GetUserLolDataAsync(userId);

            await Database.UpdateLolDataAsync(userId, lolData.SummonerRegion, lolData.PlayerUUID, lolData.AccountId,
                lolData.SummonerId, lolData.SummonerName);

            var sgUser = IonicClient.GetGuildUserById(Settings.App.MainGuildId, userId);

            if (sgUser is null)
            {
                RiftBot.Log.Warn($"[User|{userId}] SGUser is null!");
                return;
            }

            (RequestResult rankResult, LeaguePosition[] rankData) =
                await GetLeaguePositionsByEncryptedSummonerIdAsync(lolData.SummonerRegion, lolData.SummonerId);

            if (rankResult != RequestResult.Success)
            {
                RiftBot.Log.Warn($"[User|{userId}] No rank data");
                return;
            }

            var newRank = GetRankFromPosition(rankData.FirstOrDefault(x => x.QueueType == "RANKED_SOLO_5x5"));

            var currentRank = GetCurrentRank(sgUser);

            if (currentRank == newRank)
            {
                if (!silentMode)
                    await sgUser.SendEmbedAsync(LolProfileEmbeds.NoRankUpdated);
                
                return;
            }

            bool isUp = newRank > currentRank;

            if (newRank != currentRank)
                OnRankChanged?.Invoke(new RankChangedEventArgs(userId, currentRank, newRank, isUp));

            await RemoveRankedRole(sgUser, currentRank);

            ulong roleId = 0ul;

            switch (newRank)
            {
                case LeagueRank.Iron:

                    roleId = Settings.RoleId.RankIron;
                    break;

                case LeagueRank.Bronze:

                    roleId = Settings.RoleId.RankBronze;
                    break;

                case LeagueRank.Silver:

                    roleId = Settings.RoleId.RankSilver;
                    break;

                case LeagueRank.Gold:

                    roleId = Settings.RoleId.RankGold;
                    break;

                case LeagueRank.Platinum:

                    roleId = Settings.RoleId.RankPlatinum;
                    break;

                case LeagueRank.Diamond:

                    roleId = Settings.RoleId.RankDiamond;
                    break;

                case LeagueRank.Master:

                    roleId = Settings.RoleId.RankMaster;
                    break;

                case LeagueRank.GrandMaster:

                    roleId = Settings.RoleId.RankGrandmaster;
                    break;

                case LeagueRank.Challenger:

                    roleId = Settings.RoleId.RankChallenger;
                    break;
            }

            if (!IonicClient.GetRole(Settings.App.MainGuildId, roleId, out var role))
            {
                RiftBot.Log.Warn($"[User|{userId}] Failed to get role {roleId}");
                return;
            }

            await sgUser.AddRoleAsync(role);

            if (isUp)
            {
                await chatChannel.SendEmbedAsync(LolProfileEmbeds.ChatRankUpdated(sgUser, GetStatStringFromRank(newRank)));

                if (newRank < LeagueRank.Bronze)
                    return;

                await Database.AddInventoryAsync(sgUser.Id, chests: 4u);
                await sgUser.SendEmbedAsync(LolProfileEmbeds.DMRankUpdated);
            }
            else
            {
                if (!silentMode)
                    await sgUser.SendEmbedAsync(LolProfileEmbeds.NoRankUpdated);
            }

            RiftBot.Log.Debug($"[User|{userId}] Rank update completed.");
        }

        static LeagueRank GetCurrentRank(IGuildUser user)
        {
            var currentRoleIds = user.RoleIds;

            if (currentRoleIds.Contains(Settings.RoleId.RankIron))
                return LeagueRank.Iron;

            if (currentRoleIds.Contains(Settings.RoleId.RankBronze))
                return LeagueRank.Bronze;

            if (currentRoleIds.Contains(Settings.RoleId.RankSilver))
                return LeagueRank.Silver;

            if (currentRoleIds.Contains(Settings.RoleId.RankGold))
                return LeagueRank.Gold;

            if (currentRoleIds.Contains(Settings.RoleId.RankPlatinum))
                return LeagueRank.Platinum;

            if (currentRoleIds.Contains(Settings.RoleId.RankDiamond))
                return LeagueRank.Diamond;

            if (currentRoleIds.Contains(Settings.RoleId.RankMaster))
                return LeagueRank.Master;

            if (currentRoleIds.Contains(Settings.RoleId.RankGrandmaster))
                return LeagueRank.GrandMaster;

            if (currentRoleIds.Contains(Settings.RoleId.RankChallenger))
                return LeagueRank.Challenger;

            return LeagueRank.Unranked;
        }

        async Task RemoveRankedRole(SocketGuildUser sgUser, LeagueRank rank)
        {
            switch (rank)
            {
                case LeagueRank.Iron:

                    if (!IonicClient.GetRole(Settings.App.MainGuildId, Settings.RoleId.RankIron, out var ironRole))
                        return;

                    await sgUser.RemoveRoleAsync(ironRole);

                    break;

                case LeagueRank.Bronze:

                    if (!IonicClient.GetRole(Settings.App.MainGuildId, Settings.RoleId.RankBronze, out var bronzeRole))
                        return;

                    await sgUser.RemoveRoleAsync(bronzeRole);

                    break;

                case LeagueRank.Silver:

                    if (!IonicClient.GetRole(Settings.App.MainGuildId, Settings.RoleId.RankSilver, out var silverRole))
                        return;

                    await sgUser.RemoveRoleAsync(silverRole);
                    break;

                case LeagueRank.Gold:

                    if (!IonicClient.GetRole(Settings.App.MainGuildId, Settings.RoleId.RankGold, out var goldRole))
                        return;

                    await sgUser.RemoveRoleAsync(goldRole);
                    break;

                case LeagueRank.Platinum:

                    if (!IonicClient.GetRole(Settings.App.MainGuildId, Settings.RoleId.RankPlatinum, out var platRole))
                        return;

                    await sgUser.RemoveRoleAsync(platRole);
                    break;

                case LeagueRank.Diamond:

                    if (!IonicClient.GetRole(Settings.App.MainGuildId, Settings.RoleId.RankDiamond,
                                             out var diamondRole))
                        return;

                    await sgUser.RemoveRoleAsync(diamondRole);
                    break;

                case LeagueRank.Master:

                    if (!IonicClient.GetRole(Settings.App.MainGuildId, Settings.RoleId.RankMaster, out var masterRole))
                        return;

                    await sgUser.RemoveRoleAsync(masterRole);
                    break;

                case LeagueRank.GrandMaster:

                    if (!IonicClient.GetRole(Settings.App.MainGuildId, Settings.RoleId.RankGrandmaster,
                                             out var grandmasterRole))
                        return;

                    await sgUser.RemoveRoleAsync(grandmasterRole);
                    break;

                case LeagueRank.Challenger:

                    if (!IonicClient.GetRole(Settings.App.MainGuildId, Settings.RoleId.RankChallenger,
                                             out var challengerRole))
                        return;

                    await sgUser.RemoveRoleAsync(challengerRole);
                    break;
            }
        }

        async Task AssignRoleFromRankAsync(SocketGuildUser guildUser, PendingUser approvedUser)
        {
            (RequestResult rankResult, LeaguePosition[] rankData) =
                await GetLeaguePositionsByEncryptedSummonerIdAsync(approvedUser.Region, approvedUser.SummonedId);

            if (rankResult != RequestResult.Success)
                return;

            var soloqRank = GetRankFromPosition(rankData.FirstOrDefault(x => x.QueueType == "RANKED_SOLO_5x5"));

            ulong roleId = 0ul;

            switch (soloqRank)
            {
                case LeagueRank.Iron:

                    roleId = Settings.RoleId.RankIron;
                    break;

                case LeagueRank.Bronze:

                    roleId = Settings.RoleId.RankBronze;
                    break;

                case LeagueRank.Silver:

                    roleId = Settings.RoleId.RankSilver;
                    break;

                case LeagueRank.Gold:

                    roleId = Settings.RoleId.RankGold;
                    break;

                case LeagueRank.Platinum:

                    roleId = Settings.RoleId.RankPlatinum;
                    break;

                case LeagueRank.Diamond:

                    roleId = Settings.RoleId.RankDiamond;
                    break;

                case LeagueRank.Master:

                    roleId = Settings.RoleId.RankMaster;
                    break;

                case LeagueRank.GrandMaster:

                    roleId = Settings.RoleId.RankGrandmaster;
                    break;

                case LeagueRank.Challenger:

                    roleId = Settings.RoleId.RankChallenger;
                    break;
            }

            if (roleId == 0ul)
                return;

            if (!IonicClient.GetRole(Settings.App.MainGuildId, roleId, out var role))
                return;

            await guildUser.AddRoleAsync(role);
        }

        public static LeagueRank GetRankFromRoleId(ulong roleId)
        {
            if (roleId == Settings.RoleId.RankIron)
                return LeagueRank.Iron;
            if (roleId == Settings.RoleId.RankBronze)
                return LeagueRank.Bronze;
            if (roleId == Settings.RoleId.RankSilver)
                return LeagueRank.Silver;
            if (roleId == Settings.RoleId.RankGold)
                return LeagueRank.Gold;
            if (roleId == Settings.RoleId.RankPlatinum)
                return LeagueRank.Platinum;
            if (roleId == Settings.RoleId.RankDiamond)
                return LeagueRank.Diamond;
            if (roleId == Settings.RoleId.RankMaster)
                return LeagueRank.Master;
            if (roleId == Settings.RoleId.RankGrandmaster)
                return LeagueRank.GrandMaster;
            if (roleId == Settings.RoleId.RankChallenger)
                return LeagueRank.Challenger;

            return LeagueRank.Unranked;
        }

        public static LeagueRank GetRankFromPosition(LeaguePosition pos)
        {
            if (pos is null)
                return LeagueRank.Unranked;

            switch (pos.Tier)
            {
                case "IRON":        return LeagueRank.Iron;
                case "BRONZE":      return LeagueRank.Bronze;
                case "SILVER":      return LeagueRank.Silver;
                case "GOLD":        return LeagueRank.Gold;
                case "PLATINUM":    return LeagueRank.Platinum;
                case "DIAMOND":     return LeagueRank.Diamond;
                case "MASTER":      return LeagueRank.Master;
                case "GRANDMASTER": return LeagueRank.GrandMaster;
                case "CHALLENGER":  return LeagueRank.Challenger;
            }

            return LeagueRank.Unranked;
        }

        public static string GetStatStringFromRank(LeagueRank rank)
        {
            switch (rank)
            {
                case LeagueRank.Iron:        return $"{Settings.Emote.RankIron} Железо";
                case LeagueRank.Bronze:      return $"{Settings.Emote.RankBronze} Бронза";
                case LeagueRank.Silver:      return $"{Settings.Emote.RankSilver} Серебро";
                case LeagueRank.Gold:        return $"{Settings.Emote.RankGold} Золото";
                case LeagueRank.Platinum:    return $"{Settings.Emote.RankPlatinum} Платина";
                case LeagueRank.Diamond:     return $"{Settings.Emote.RankDiamond} Алмаз";
                case LeagueRank.Master:      return $"{Settings.Emote.RankMaster} Мастер";
                case LeagueRank.GrandMaster: return $"{Settings.Emote.RankGrandmaster} Грандмастер";
                case LeagueRank.Challenger:  return $"{Settings.Emote.RankChallenger} Претендент";
            }

            return string.Empty;
        }

        #endregion Rank

        public async Task<(RequestResult, Summoner)> GetSummonerByEncryptedSummonerIdAsync(
            string region, string encryptedSummonerId)
        {
            try
            {
                var result =
                    await api.SummonerV4.GetBySummonerIdAsync(GetRegionFromString(region), encryptedSummonerId);

                return (RequestResult.Success, result);
            }
            catch (Exception ex)
            {
                RiftBot.Log.Error(ex);
                return (RequestResult.Error, null);
            }
        }

        public async Task<(RequestResult, Summoner)> GetSummonerByEncryptedUUIDAsync(
            string region, string encryptedUUID)
        {
            try
            {
                var result = await api.SummonerV4.GetByPUUIDAsync(GetRegionFromString(region), encryptedUUID);

                return (RequestResult.Success, result);
            }
            catch (Exception ex)
            {
                RiftBot.Log.Error(ex);
                return (RequestResult.Error, null);
            }
        }

        public async Task<(RequestResult, Summoner)> GetSummonerByNameAsync(string region, string name)
        {
            try
            {
                var result = await api.SummonerV4.GetBySummonerNameAsync(GetRegionFromString(region), name);

                return (RequestResult.Success, result);
            }
            catch (Exception ex)
            {
                RiftBot.Log.Error(ex);
                return (RequestResult.Error, null);
            }
        }

        public async Task<(RequestResult, LeaguePosition[])> GetLeaguePositionsByEncryptedSummonerIdAsync(
            string region, string encryptedSummonerId)
        {
            try
            {
                var result = await api.LeagueV4.GetAllLeaguePositionsForSummonerAsync(GetRegionFromString(region),
                                                                                      encryptedSummonerId);

                return (RequestResult.Success, result);
            }
            catch (Exception ex)
            {
                RiftBot.Log.Error(ex);
                return (RequestResult.Error, null);
            }
        }

        public async Task<(RequestResult, string)> GetThirdPartyCodeByEncryptedSummonerIdAsync(string region, string
                                                                                                   encryptedSummonerId)
        {
            try
            {
                var result =
                    await api.ThirdPartyCodeV4.GetThirdPartyCodeBySummonerIdAsync(GetRegionFromString(region),
                                                                                  encryptedSummonerId);

                return (RequestResult.Success, result);
            }
            catch (Exception ex)
            {
                RiftBot.Log.Error(ex);
                return (RequestResult.Error, null);
            }
        }

        public async Task<(RequestResult, MatchReference[])> GetLast20MatchesByAccountIdAsync(string region, string
                                                                                                  encryptedAccountId)
        {
            try
            {
                var matchlist =
                    await api.MatchV4.GetMatchlistAsync(GetRegionFromString(region), encryptedAccountId, beginIndex: 0,
                                                        endIndex: 19);

                return (RequestResult.Success, matchlist.Matches);
            }
            catch (Exception ex)
            {
                RiftBot.Log.Error(ex);
                return (RequestResult.Error, null);
            }
        }

        public async Task<(RequestResult, Match)> GetMatchById(string region, long matchId)
        {
            try
            {
                var match = await api.MatchV4.GetMatchAsync(GetRegionFromString(region), matchId);

                return (RequestResult.Success, match);
            }
            catch (Exception ex)
            {
                RiftBot.Log.Error(ex);
                return (RequestResult.Error, null);
            }
        }
        
        static Region GetRegionFromString(string regionName)
        {
            switch (regionName.ToLowerInvariant())
            {
                case "ru":   return Region.RU;
                case "euw":  return Region.EUW;
                case "eune": return Region.EUNE;

                default:
                    RiftBot.Log.Error($"Invalid region: {regionName}");
                    throw new NotSupportedException($"Invalid region: {regionName}");
            }
        }

        public string GetSummonerIconUrlById(int iconId)
        {
            return $"http://ddragon.leagueoflegends.com/cdn/{Settings.App.LolVersion}/img/profileicon/{iconId.ToString()}.png";
        }

        public static string GetChampionSquareByName(ChampionImage ci)
        {
            return String.Format(DataDragonChampPortraitTemplate, Settings.App.LolVersion, ci.Full);
        }

        public string GetQueueNameById(int id)
        {
            switch (id)
            {
                case 0: return "Кастомная игра";

                case 70:
                case 72:
                case 73:
                case 75:
                case 76:
                case 78:
                case 83:
                case 98:
                case 310:
                case 313:
                case 317:
                case 325:
                case 600:
                case 610:
                case 900:
                case 910:
                case 920:
                case 940:
                case 950:
                case 960:
                case 980:
                case 990:
                case 1000:
                case 1010:
                case 1020:
                case 1030:
                case 1040:
                case 1050:
                case 1060:
                case 1070: return "Специальный";

                case 400:
                case 430:
                case 460: return "Обычная очередь";

                case 420: return "Ранговая (одиночная)";

                case 440:
                case 470: return "Ранговая (гибкая)";

                case 100:
                case 450: return "ARAM";

                case 800:
                case 810:
                case 820:
                case 830:
                case 840:
                case 850: return "Катка с ботами";

                case 1200: return "Осада Нексуса";

                default:
                    RiftBot.Log.Warn($"Queue ID is not known: {id}");
                    return "Неизвестная очередь";
            }
        }

        static async Task<GetResponse> GetAsync(string url)
        {
            var request = (HttpWebRequest) WebRequest.Create(url);

            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse) await request.GetResponseAsync();
            }
            catch (WebException we)
            {
                response = (HttpWebResponse) we.Response;
            }

            HttpStatusCode statusCode = response.StatusCode;

            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                return new GetResponse((int) statusCode, await reader.ReadToEndAsync());
            }
        }
    }

    public enum RequestResult
    {
        Success,
        Error,
        RateLimited,
    }

    public enum LeagueRank
    {
        Unranked,
        Iron,
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Master,
        GrandMaster,
        Challenger,
    }

    public class GetResponse
    {
        public readonly int StatusCode;
        public readonly string Content;

        public GetResponse(int statusCode, string content)
        {
            StatusCode = statusCode;
            Content = content;
        }
    }

    public class Champion
    {
        [JsonProperty("data")]
        public Dictionary<string, ChampionData> Data { get; set; }
    }

    public class ChampionData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("image")]
        public ChampionImage Image { get; set; }
    }

    public class ChampionImage
    {
        [JsonProperty("full")]
        public string Full { get; set; }
    }
}
