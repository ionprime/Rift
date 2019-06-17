using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Rift.Configuration;
using Rift.Data;
using Rift.Services.Economy;
using Rift.Services.Message;
using Rift.Services.Reward;
using Rift.Util;

using Discord;
using Humanizer;
using IonicLib;
using IonicLib.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Rift.Services
{
    public class EconomyService
    {
        public static readonly Dictionary<ulong, string> TempRoles = new Dictionary<ulong, string>
        {
            {
                Settings.RoleId.Arcade, "Аркадные"
            },
            {
                Settings.RoleId.Arclight, "Светоносные"
            },
            {
                Settings.RoleId.BloodMoon, "Кровавая луна"
            },
            {
                Settings.RoleId.BravePoro, "Храбрые поро"
            },
            {
                Settings.RoleId.Chosen, "Избранные"
            },
            {
                Settings.RoleId.DarkStar, "Темная звезда"
            },
            {
                Settings.RoleId.Debonairs, "Галантные"
            },
            {
                Settings.RoleId.Epic, "Эпические"
            },
            {
                Settings.RoleId.HappyPoro, "Довольные поро"
            },
            {
                Settings.RoleId.Hextech, "Хекстековые"
            },
            {
                Settings.RoleId.Justicars, "Юстициары"
            },
            {
                Settings.RoleId.Mythic, "Мифические"
            },
            {
                Settings.RoleId.Party, "Тусовые"
            },
            {
                Settings.RoleId.Pentakill, "Pentakill"
            },
            {
                Settings.RoleId.StarGuardians, "Звездные защитники"
            },
            {
                Settings.RoleId.ThunderLords, "Повелители грома"
            },
            {
                Settings.RoleId.Vandals, "Вандалы"
            },
            {
                Settings.RoleId.Victorious, "Победоносные"
            },
            {
                Settings.RoleId.Ward, "Вардилочка"
            },
            {
                Settings.RoleId.Reworked, "Реворкнутый"
            },
            {
                Settings.RoleId.Meta, "Метовый"
            },
            {
                Settings.RoleId.Hasagi, "Хасаги"
            },
            {
                Settings.RoleId.YasuoPlayer, "Ясуоплееры"
            },
        };

        static Timer ratingUpdateTimer;
        static Timer ActiveUsersTimer;
        static Timer RichUsersTimer;
        static readonly TimeSpan ratingTimerCooldown = TimeSpan.FromHours(1);
        static List<ulong> ratingSorted = null;

        static SemaphoreSlim chestMutex = new SemaphoreSlim(1);
        static SemaphoreSlim capsuleMutex = new SemaphoreSlim(1);
        static SemaphoreSlim sphereMutex = new SemaphoreSlim(1);
        static SemaphoreSlim storeMutex = new SemaphoreSlim(1);
        static SemaphoreSlim dailyChestMutex = new SemaphoreSlim(1);
        static SemaphoreSlim bragMutex = new SemaphoreSlim(1);

        public void Init()
        {
            ratingUpdateTimer = new Timer(async delegate { await UpdateRatingAsync(); }, null, TimeSpan.FromMinutes(5), ratingTimerCooldown);
            InitActiveUsersTimer();
            InitRichUsersTimer();
        }

        static void InitActiveUsersTimer()
        {
            var today = DateTime.Today.AddHours(16);

            if (DateTime.UtcNow > today)
                today = today.AddDays(1);

            ActiveUsersTimer = new Timer(async delegate { await ShowActiveUsersAsync(); }, null,
                today - DateTime.UtcNow, TimeSpan.FromDays(1));
        }

        void InitRichUsersTimer()
        {
            var today = DateTime.Today.AddHours(18);

            if (DateTime.UtcNow > today)
                today = today.AddDays(1);

            RichUsersTimer = new Timer(async delegate { await ShowRichUsersAsync(); }, null,
                today - DateTime.UtcNow, TimeSpan.FromDays(1));
        }

        public static async Task ShowActiveUsersAsync()
        {
            if (!IonicClient.GetTextChannel(Settings.App.MainGuildId, Settings.ChannelId.Comms, out var commsChannel))
                return;

            var topTen = await Database.GetTopTenByExpAsync(x => 
                !(IonicClient.GetGuildUserById(Settings.App.MainGuildId, x.UserId) is null));

            if (topTen.Length == 0)
                return;

            var msg = await RiftBot.GetMessageAsync("economy-activeusers", new FormatData
            {
                Economy = new EconomyData
                {
                    Top10Exp = topTen
                }
            });
            await commsChannel.SendIonicMessageAsync(msg);
        }

        public static async Task ShowRichUsersAsync()
        {
            if (!IonicClient.GetTextChannel(Settings.App.MainGuildId, Settings.ChannelId.Comms, out var commsChannel))
                return;

            var topTen = await Database.GetTopTenByCoinsAsync(x => 
                !(IonicClient.GetGuildUserById(Settings.App.MainGuildId, x.UserId) is null));

            if (topTen.Length == 0)
                return;

            var msg = await RiftBot.GetMessageAsync("economy-richusers", new FormatData
            {
                Economy = new EconomyData
                {
                    Top10Coins = topTen
                }
            });
            await commsChannel.SendIonicMessageAsync(msg);
        }

        public async Task ProcessMessageAsync(IUserMessage message)
        {
            await AddExpAsync(message.Author.Id, 1u).ConfigureAwait(false);
            await CheckDailyMessageAsync(message.Author.Id).ConfigureAwait(false);
        }

        static async Task AddExpAsync(ulong userId, uint exp)
        {
            await Database.AddExperienceAsync(userId, exp)
                .ContinueWith(async _ => await CheckLevelUpAsync(userId));
        }

        static async Task CheckLevelUpAsync(ulong userId)
        {
            var dbUser = await Database.GetUserAsync(userId);

            if (dbUser.Experience != uint.MaxValue)
            {
                var newLevel = dbUser.Level + 1u;

                while (dbUser.Experience >= GetExpForLevel(newLevel))
                {
                    newLevel++;
                }

                newLevel--;

                if (newLevel > dbUser.Level)
                {
                    await Database.SetLevelAsync(dbUser.UserId, newLevel);

                    RiftBot.Log.Info($"{dbUser.UserId.ToString()} just leveled up: {dbUser.Level.ToString()} => {newLevel.ToString()}");

                    if (newLevel == 1u)
                        return; //no rewards on level 1

                    await GiveRewardsForLevelAsync(dbUser.UserId, dbUser.Level, newLevel);
                }
            }
        }

        static async Task CheckDailyMessageAsync(ulong userId)
        {
            return;

            try
            {
                var dbDaily = await Database.GetUserCooldownsAsync(userId);
    
                var diff = DateTime.UtcNow - dbDaily.LastDailyChestTime;
    
                if (diff.Days == 0)
                    return;
            
                await dailyChestMutex.WaitAsync().ConfigureAwait(false);

                var sgUser = IonicClient.GetGuildUserById(Settings.App.MainGuildId, userId);

                if (sgUser is null)
                    return;

                var coins = Helper.NextUInt(1000, 2001);
                var tokens = 0u;

                if (IonicClient.HasRolesAny(sgUser, Settings.RoleId.Legendary))
                    coins += 1000u;
                if (IonicClient.HasRolesAny(sgUser, Settings.RoleId.Absolute))
                    coins += 2000u;
                if (IonicClient.HasRolesAny(sgUser, Settings.RoleId.Keepers))
                    tokens += 1u;
                if (IonicClient.HasRolesAny(sgUser, Settings.RoleId.Active))
                    coins += 500u;

                var reward = new ItemReward().AddCoins(coins).AddTokens(tokens);

                await reward.DeliverToAsync(userId);
                await Database.SetLastDailyChestTimeAsync(userId, DateTime.UtcNow);

                var msg = await RiftBot.GetMessageAsync("daily-reward", new FormatData(userId));

                RiftBot.GetService<MessageService>().TryAddSend(
                    new MixedMessage(DestinationType.GuildChannel, Settings.ChannelId.Comms, TimeSpan.Zero, msg));
            }
            finally
            {
                dailyChestMutex.Release();
            }
        }

        static async Task GiveRewardsForLevelAsync(ulong userId, uint fromLevel, uint toLevel)
        {
            for (uint level = fromLevel + 1; level <= toLevel; level++)
            {
                await GiveRewardsForLevelAsync(userId, level);
            }
        }

        static async Task GiveRewardsForLevelAsync(ulong userId, uint level)
        {
            var sgUser = IonicClient.GetGuildUserById(Settings.App.MainGuildId, userId);

            if (sgUser is null)
                return;

            ItemReward reward;

            if (level == 100u || level == 50u)
                reward = new ItemReward().AddCapsules(1u);
            else if (level % 25u == 0u)
                reward = new ItemReward().AddSpheres(1u);
            else if (level % 10u == 0u)
                reward = new ItemReward().AddTokens(2u);
            else if (level % 5u == 0u)
                reward = new ItemReward().AddCoins(2_000u).AddTickets(1u);
            else 
                reward = new ItemReward().AddCoins(2_000u).AddChests(1u);

            await reward.DeliverToAsync(userId);
        }

        public async Task<IonicMessage> GetUserCooldownsAsync(ulong userId)
        {
            return await RiftBot.GetMessageAsync("user-cooldowns", new FormatData(userId));
        }
        
        public async Task<IonicMessage> GetUserProfileAsync(ulong userId)
        {
            return await RiftBot.GetMessageAsync("user-profile", new FormatData(userId));
        }

        public async Task<IonicMessage> GetUserGameStatAsync(ulong userId)
        {
            var sgUser = IonicClient.GetGuildUserById(Settings.App.MainGuildId, userId);

            if (sgUser is null)
                return MessageService.Error;

            var dbSummoner = await Database.GetUserLolDataAsync(userId);

            if (string.IsNullOrWhiteSpace(dbSummoner.PlayerUUID))
                return await RiftBot.GetMessageAsync("loldata-nodata", new FormatData(userId));

            (var summonerResult, var summoner) = await RiftBot.GetService<RiotService>()
                .GetSummonerByEncryptedSummonerIdAsync(dbSummoner.SummonerRegion, dbSummoner.SummonerId);

            if (summonerResult != RequestResult.Success)
                return MessageService.Error;

            (var requestResult, var leaguePositions) = await RiftBot.GetService<RiotService>()
                .GetLeaguePositionsByEncryptedSummonerIdAsync(dbSummoner.SummonerRegion, dbSummoner.SummonerId);

            if (requestResult != RequestResult.Success)
                return MessageService.Error;

            return await RiftBot.GetMessageAsync("loldata-stat-success", new FormatData(userId)
            {
                LolStat = new LolStatData
                {
                    Summoner = summoner,
                    SoloQueue = leaguePositions.FirstOrDefault(x => x.QueueType == "RANKED_SOLO_5x5"),
                    Flex5v5 = leaguePositions.FirstOrDefault(x => x.QueueType == "RANKED_FLEX_5x5"),
                }
            });
        }

        public async Task<IonicMessage> GetUserStatAsync(ulong userId)
        {
            var statistics = await Database.GetUserStatisticsAsync(userId);

            return await RiftBot.GetMessageAsync("user-stat", new FormatData(userId)
            {
                Statistics = statistics
            });
        }

        static async Task UpdateRatingAsync()
        {
            using (var context = new RiftContext())
            {
                var sortedIds = await context.Users
                    .OrderByDescending(x => x.Level)
                    .ThenByDescending(x => x.Experience)
                    .Select(x => x.UserId)
                    .ToListAsync();

                ratingSorted = sortedIds;
            }
        }

        public async Task<IonicMessage> OpenChestAsync(ulong userId, uint amount)
        {
            await chestMutex.WaitAsync().ConfigureAwait(false);

            IonicMessage result;

            RiftBot.Log.Info($"Opening chest for {userId.ToString()}.");

            try
            {
                result = await OpenChestInternalAsync(userId, amount).ConfigureAwait(false);
            }
            finally
            {
                chestMutex.Release();
            }

            return result;
        }

        public async Task<IonicMessage> OpenChestAllAsync(ulong userId)
        {
            await chestMutex.WaitAsync().ConfigureAwait(false);

            IonicMessage result;

            RiftBot.Log.Info($"Opening all chests for {userId.ToString()}");

            try
            {
                var dbInventory = await Database.GetUserInventoryAsync(userId);
                result = await OpenChestInternalAsync(userId, dbInventory.Chests).ConfigureAwait(false);
            }
            finally
            {
                chestMutex.Release();
            }

            return result;
        }

        static async Task<IonicMessage> OpenChestInternalAsync(ulong userId, uint amount)
        {
            var dbInventory = await Database.GetUserInventoryAsync(userId);
            var sgUser = IonicClient.GetGuildUserById(Settings.App.MainGuildId, userId);

            if (dbInventory.Chests < amount || amount == 0)
                return await RiftBot.GetMessageAsync("chests-nochests", new FormatData(userId));

            await Database.RemoveInventoryAsync(userId, new InventoryData { Chests = amount });

            var chest = new ChestReward();
            await chest.DeliverToAsync(userId);
            await Database.AddStatisticsAsync(userId, chestsOpenedTotal: amount);

            return await RiftBot.GetMessageAsync("chests-open-success", new FormatData(userId));
        }

        public async Task<IonicMessage> OpenCapsuleAsync(ulong userId)
        {
            await capsuleMutex.WaitAsync().ConfigureAwait(false);

            IonicMessage result;

            RiftBot.Log.Info($"Opening capsule for {userId.ToString()}.");

            try
            {
                result = await OpenCapsuleInternalAsync(userId).ConfigureAwait(false);
            }
            finally
            {
                capsuleMutex.Release();
            }

            return result;
        }

        static async Task<IonicMessage> OpenCapsuleInternalAsync(ulong userId)
        {
            var dbUserInventory = await Database.GetUserInventoryAsync(userId);

            if (dbUserInventory.Capsules == 0u)
                await RiftBot.GetMessageAsync("capsules-nocapsules", new FormatData(userId));

            await Database.RemoveInventoryAsync(userId, new InventoryData { Capsules = 1u });

            var capsule = new CapsuleReward();
            await capsule.DeliverToAsync(userId);
            await Database.AddStatisticsAsync(userId, capsuleOpenedTotal: 1u);

            return await RiftBot.GetMessageAsync("capsules-open-success", new FormatData(userId));
        }

        public async Task<IonicMessage> OpenSphereAsync(ulong userId)
        {
            await sphereMutex.WaitAsync().ConfigureAwait(false);

            IonicMessage result;

            RiftBot.Log.Info($"Opening sphere for {userId.ToString()}.");

            try
            {
                result = await OpenSphereInternalAsync(userId).ConfigureAwait(false);
            }
            finally
            {
                sphereMutex.Release();
            }

            return result;
        }

        static async Task<IonicMessage> OpenSphereInternalAsync(ulong userId)
        {
            var dbInventory = await Database.GetUserInventoryAsync(userId);

            if (dbInventory.Spheres == 0u)
                return await RiftBot.GetMessageAsync("spheres-nospheres", new FormatData(userId));

            await Database.RemoveInventoryAsync(userId, new InventoryData { Spheres = 1u });

            var sphere = new SphereReward();
            await sphere.DeliverToAsync(userId);
            await Database.AddStatisticsAsync(userId, sphereOpenedTotal: 1u);

            return await RiftBot.GetMessageAsync("spheres-open-success", new FormatData(userId));
        }

        public async Task<IonicMessage> StorePurchaseAsync(ulong userId, StoreItem item)
        {
            await storeMutex.WaitAsync().ConfigureAwait(false);

            RiftBot.Log.Info($"Store purchase: #{item.Id.ToString()} by {userId.ToString()}.");

            IonicMessage result;
            
            try
            {
                result = await StorePurchaseInternalAsync(userId, item).ConfigureAwait(false);
            }
            finally
            {
                storeMutex.Release();
            }

            return result;
        }

        static async Task<IonicMessage> StorePurchaseInternalAsync(ulong userId, StoreItem item)
        {
            var sgUser = IonicClient.GetGuildUserById(Settings.App.MainGuildId, userId);

            if (sgUser is null)
                return MessageService.Error;

            (var canPurchase, var remaining) = await CanBuyStoreAsync(userId);

            if (!RiftBot.IsAdmin(sgUser) && !canPurchase)
                await RiftBot.GetMessageAsync("store-cooldown", new FormatData(userId));

            // if buying temp role over existing one
            if (item.Type == StoreItemType.TempRole)
            {
                var userTempRoles = await RiftBot.GetService<RoleService>().GetUserTempRolesAsync(userId);

                if (userTempRoles != null && userTempRoles.Count > 0)
                {
                    if (userTempRoles.Any(x => x.UserId == userId && x.RoleId == item.RoleId))
                    {
                        return await RiftBot.GetMessageAsync("store-hasrole", new FormatData(userId));
                    }
                }
            }

            (var result, var currencyType) = await WithdrawCurrencyAsync();

            if (!result)
            {
                switch (currencyType)
                {
                    case Currency.Coins: return await RiftBot.GetMessageAsync("store-nocoins", new FormatData(userId));
                    case Currency.Tokens: return await RiftBot.GetMessageAsync("store-notokens", new FormatData(userId));
                }
            }

            if (!IonicClient.GetTextChannel(Settings.App.MainGuildId, Settings.ChannelId.Comms, out var channel))
                return MessageService.Error;

            switch (item.Type)
            {
                case StoreItemType.DoubleExp:
                    await Database.AddInventoryAsync(userId, new InventoryData { DoubleExps = 1u });
                    break;

                case StoreItemType.Capsule:
                    await Database.AddInventoryAsync(userId, new InventoryData { Capsules = 1u });
                    break;

                case StoreItemType.Ticket:
                    await Database.AddInventoryAsync(userId, new InventoryData{  Tickets = 1u });
                    break;

                case StoreItemType.Chest:
                    await Database.AddInventoryAsync(userId, new InventoryData { Chests = 1u });
                    break;

                case StoreItemType.Token:
                    await Database.AddInventoryAsync(userId, new InventoryData { Tokens = 1u });
                    break;

                case StoreItemType.Sphere:
                    await Database.AddInventoryAsync(userId, new InventoryData { Spheres = 1u });
                    break;

                case StoreItemType.BotRespect:
                    await Database.AddInventoryAsync(userId, new InventoryData { BotRespects = 1u });
                    break;

                case StoreItemType.TempRole:

                    await RiftBot.GetService<RoleService>()
                        .AddTempRoleAsync(userId, item.RoleId, TimeSpan.FromDays(30), "Store Purchase");

                    var msgTempRole = await RiftBot.GetMessageAsync("store-purchased-temprole", new FormatData(userId));
                    await channel.SendIonicMessageAsync(msgTempRole);
                    break;

                case StoreItemType.PermanentRole:
                    var msgPermRole = await RiftBot.GetMessageAsync("store-purchased-permrole", new FormatData(userId));
                    await channel.SendIonicMessageAsync(msgPermRole);
                    await RiftBot.GetService<RoleService>().AddPermanentRoleAsync(userId, item.RoleId);
                    break;
            }

            async Task<(bool, Currency)> WithdrawCurrencyAsync()
            {
                var dbInventory = await Database.GetUserInventoryAsync(userId);

                switch (item.Currency)
                {
                    case Currency.Coins:
                    {
                        if (dbInventory.Coins < item.Price)
                            return (false, item.Currency);

                        await Database.RemoveInventoryAsync(userId, new InventoryData { Coins = item.Price } );
                        break;
                    }

                    case Currency.Tokens:
                    {
                        if (dbInventory.Tokens < item.Price)
                            return (false, item.Currency);

                        await Database.RemoveInventoryAsync(userId, new InventoryData { Tokens = item.Price });
                        break;
                    }
                }

                return (true, item.Currency);
            }

            await Database.SetLastStoreTimeAsync(userId, DateTime.UtcNow);
            await Database.AddStatisticsAsync(userId, purchasedItemsTotal: 1u);

            return await RiftBot.GetMessageAsync("store-success", new FormatData(userId));
        }

        static async Task<(bool, TimeSpan)> CanBuyStoreAsync(ulong userId)
        {
            var dbStore = await Database.GetUserCooldownsAsync(userId);

            if (dbStore.LastStoreTime == DateTime.MinValue)
                return (true, TimeSpan.Zero);

            var diff = DateTime.UtcNow - dbStore.LastStoreTime;
            var remaining = Settings.Economy.StoreCooldown - diff;

            return (diff > Settings.Economy.StoreCooldown, remaining);
        }

        

        public async Task ActivateDoubleExp(ulong userId)
        {
            if (!IonicClient.GetTextChannel(Settings.App.MainGuildId, Settings.ChannelId.Comms, out var channel))
                return;

            var dbInventory = await Database.GetUserInventoryAsync(userId);

            if (dbInventory.BonusDoubleExp == 0)
            {
                var msg = await RiftBot.GetMessageAsync("activate-nopowerup", new FormatData(userId));
                await channel.SendIonicMessageAsync(msg);
                return;
            }

            var dbDoubleExp = await Database.GetUserCooldownsAsync(userId);
            if (dbDoubleExp.DoubleExpTime > DateTime.UtcNow)
            {
                var msg = await RiftBot.GetMessageAsync("activate-active", new FormatData(userId));
                await channel.SendIonicMessageAsync(msg);
                return;
            }

            await Database.RemoveInventoryAsync(userId, new InventoryData { DoubleExps = 1 });

            var dateTime = DateTime.UtcNow.AddHours(12.0);
            await Database.SetDoubleExpTimeAsync(userId, dateTime);

            var msgSuccess = await RiftBot.GetMessageAsync("activate-success-doubleexp", new FormatData(userId));
            await channel.SendIonicMessageAsync(msgSuccess);
        }

        public async Task ActivateBotRespect(ulong userId)
        {
            if (!IonicClient.GetTextChannel(Settings.App.MainGuildId, Settings.ChannelId.Comms, out var channel))
                return;

            var dbInventory = await Database.GetUserInventoryAsync(userId);

            if (dbInventory.BonusBotRespect == 0)
            {
                var msgNoPowerUp = await RiftBot.GetMessageAsync("activate-nopowerup", new FormatData(userId));
                await channel.SendIonicMessageAsync(msgNoPowerUp);
                return;
            }

            var dbCooldowns = await Database.GetUserCooldownsAsync(userId);
            if (dbCooldowns.BotRespectTime > DateTime.UtcNow)
            {
                var msgActive = await RiftBot.GetMessageAsync("activate-active", new FormatData(userId));
                await channel.SendIonicMessageAsync(msgActive);
                return;
            }

            await Database.RemoveInventoryAsync(userId, new InventoryData { BotRespects = 1 });

            var dateTime = DateTime.UtcNow.AddHours(12.0);
            await Database.SetBotRespeсtTimeAsync(userId, dateTime);

            var msgSuccess = await RiftBot.GetMessageAsync("activate-success-botrespect", new FormatData(userId));
            await channel.SendIonicMessageAsync(msgSuccess);
        }

        public static uint GetExpForLevel(uint level)
        {
            return (uint) (Math.Pow(level, 1.5) * 40 - 40);
        }

        public static string GetUserNameById(ulong userId)
        {
            var sgUser = IonicClient.GetGuildUserById(Settings.App.MainGuildId, userId);

            if (!(sgUser is null))
            {
                return string.IsNullOrWhiteSpace(sgUser.Nickname)
                    ? sgUser.Username
                    : sgUser.Nickname;
            }

            return "-";
        }
    }

    public enum GiftSource
    {
        Streamer,
        Voice,
        Moderator,
    }

    public enum Currency
    {
        Coins,
        Tokens,
    }
}
