using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Settings = Rift.Configuration.Settings;

using Rift.Preconditions;
using Rift.Services;
using Rift.Services.Message;
using Rift.Services.Reward;
using Rift.Util;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using IonicLib;
using IonicLib.Util;

namespace Rift.Modules
{
    public class AdminHelperModule : RiftModuleBase
    {
        readonly RiotService riotService;
        readonly EconomyService economyService;
        readonly EventService eventService;
        readonly GiveawayService giveawayService;

        public AdminHelperModule(RiotService riotService,
                                 EconomyService economyService,
                                 EventService eventService,
                                 GiveawayService giveawayService)
        {
            this.riotService = riotService;
            this.economyService = economyService;
            this.eventService = eventService;
            this.giveawayService = giveawayService;
        }

        [Command("reward")]
        [RequireAdmin]
        [RequireContext(ContextType.Guild)]
        public async Task Reward(int id)
        {
            var dbReward = await DB.Rewards.GetAsync(id);

            if (dbReward is null)
            {
                await ReplyAsync($"No such reward with ID {id}!");
                return;
            }

            var reward = dbReward.ToRewardBase();

            await reward.DeliverToAsync(Context.User.Id);

            await RiftBot.SendMessageAsync("chests-open-success", Settings.ChannelId.Commands, new FormatData(Context.User.Id)
            {
                Reward = reward
            });
        }
        
        [Command("nitrorewards")]
        [RequireAdmin]
        [RequireContext(ContextType.Guild)]
        public async Task NitroRewards()
        {
            var reward = new ItemReward().AddTokens(10u);

            var role = await DB.Roles.GetAsync(91);

            if (!IonicClient.GetRole(Settings.App.MainGuildId, role.RoleId, out var gr))
                return;

            if (!(gr is SocketRole sr))
                return;

            foreach (var sgUser in sr.Members)
            {
                await reward.DeliverToAsync(sgUser.Id);
            }

            await RiftBot.SendMessageAsync("nitro-booster-reward", Settings.ChannelId.Chat, null);
        }

        [Command("msg")]
        [RequireAdmin]
        [RequireContext(ContextType.Guild)]
        public async Task GetMsgByMapping(string mapping)
        {
            await RiftBot.SendMessageAsync(mapping, Context.Channel.Id, new FormatData(Context.User.Id));
        }

        [Command("addback")]
        [RequireAdmin]
        [RequireContext(ContextType.Guild)]
        public async Task AddBack(ulong roleId, int backId)
        {
            var dbBack = await DB.ProfileBackgrounds.GetAsync(backId);

            if (dbBack is null
                || !IonicClient.GetRole(Settings.App.MainGuildId, roleId, out var sr)
                || !(sr is SocketRole role))
            {
                await RiftBot.SendMessageAsync(MessageService.RoleNotFound, Settings.ChannelId.Commands);
                return;
            }

            var count = 0;

            foreach (var user in role.Members)
            {
                if (!await DB.Users.EnsureExistsAsync(user.Id))
                {
                    await RiftBot.SendMessageAsync(MessageService.Error, Settings.ChannelId.Commands);
                    return;
                }

                if (await DB.BackgroundInventory.HasAsync(user.Id, dbBack.Id))
                    continue;

                await DB.BackgroundInventory.AddAsync(user.Id, dbBack.Id);
                count++;
            }

            await ReplyAsync($"Added background \"{dbBack.Name}\" for role \"{role.Name}\" ({count.ToString()} user(s)).");
        }

        [Command("addrole")]
        [RequireAdmin]
        [RequireContext(ContextType.Guild)]
        public async Task AddRole(ulong roleId)
        {
            var dbRole = await DB.Roles.GetByRoleIdAsync(roleId);

            if (dbRole is null
                || !IonicClient.GetRole(Settings.App.MainGuildId, roleId, out var sr)
                || !(sr is SocketRole role))
            {
                await RiftBot.SendMessageAsync(MessageService.RoleNotFound, Settings.ChannelId.Commands);
                return;
            }

            var count = 0;

            foreach (var user in role.Members)
            {
                await user.RemoveRoleAsync(sr);

                if (!await DB.Users.EnsureExistsAsync(user.Id))
                {
                    await RiftBot.SendMessageAsync(MessageService.Error, Settings.ChannelId.Commands);
                    return;
                }

                if (await DB.RoleInventory.HasAnyAsync(user.Id, dbRole.Id))
                    continue;

                await DB.RoleInventory.AddAsync(user.Id, dbRole.Id, "Launch seeding");
                count++;
            }

            await ReplyAsync($"Added role \"{role.Name}\" to {count.ToString()} user(s).");
        }

        [Command("activestages")]
        [RequireAdmin]
        [RequireContext(ContextType.Guild)]
        public async Task ActiveStages()
        {
            var things = await DB.Quests.GetStageIdsInProgressAsync(Context.User.Id);

            await ReplyAsync($"{things.Count.ToString()} stage(s).");
        }

        [Command("estart")]
        [RequireDeveloper]
        [RequireContext(ContextType.Guild)]
        public async Task EventStart(string eventType)
        {
            await eventService.StartAsync(eventType, Context.User.Id);
        }

        [Command("gtstart")]
        [RequireTicketKeeper]
        [RequireContext(ContextType.Guild)]
        public async Task GiveawayStart(int rewardId)
        {
            await giveawayService.StartTicketGiveawayAsync(rewardId, Context.User.Id);
        }

        [Command("gastart")]
        [RequireAdmin]
        [RequireContext(ContextType.Guild)]
        public async Task GiveawayStart(string name)
        {
            await giveawayService.StartGiveawayAsync(name, Context.User.Id);
        }

        [Command("templates")]
        [RequireDeveloper]
        [RequireContext(ContextType.Guild)]
        public async Task GetTemplates()
        {
            var templates = RiftBot.GetService<MessageService>().GetActiveTemplates();

            await ReplyAsync("**Supported templates**\n\n" +
                             $"{string.Join(',', templates.Select(x => x.Template))}");
        }

        [Command("reactions")]
        [RequireDeveloper]
        [RequireContext(ContextType.Guild)]
        public async Task Reactions(ulong messageId)
        {
            var msg = (IUserMessage) await Context.Channel.GetMessageAsync(messageId);
            var text = "";

            if (msg.Reactions is null)
                text = "0";
            else
                foreach ((var emote, var reactionMetadata) in msg.Reactions)
                    text += $"**{emote}**: {reactionMetadata.ReactionCount.ToString()}\n";

            await Context.Channel.SendMessageAsync(text).ConfigureAwait(false);
        }

        [Command("exp")]
        [RequireAdmin]
        [RequireContext(ContextType.Guild)]
        public async Task Exp(uint level)
        {
            await ReplyAsync($"Level {level.ToString()}: {EconomyService.GetExpForLevel(level).ToString()} XP")
                .ConfigureAwait(false);
        }

        [Command("getprofile")]
        [RequireAdmin]
        [RequireContext(ContextType.Guild)]
        public async Task GetProfile(IUser user)
        {
            await economyService.GetUserProfileAsync(user.Id);
        }

        [Command("getgamestat")]
        [RequireAdmin]
        [RequireContext(ContextType.Guild)]
        public async Task GetGameStat(IUser user)
        {
            var message = await riotService.GetUserGameStatAsync(user.Id);

            if (message is null)
                return;

            await Context.Channel.SendIonicMessageAsync(message).ConfigureAwait(false);
        }

        [Command("getstat")]
        [RequireAdmin]
        [RequireContext(ContextType.Guild)]
        public async Task GetStat(IUser user)
        {
            await economyService.GetUserStatAsync(user.Id);
        }

        [Command("update")]
        [RequireAdmin]
        [RateLimit(1, 10, Measure.Minutes)]
        [RequireContext(ContextType.Guild)]
        public async Task Update(IUser user)
        {
            await riotService.UpdateSummonerAsync(user.Id).ConfigureAwait(false);
        }

        [Command("gastart")]
        [RequireAdmin]
        [RequireContext(ContextType.Guild)]
        public async Task StartGiveaway(string name)
        {
            await RiftBot.GetService<GiveawayService>().StartGiveawayAsync(name, Context.User.Id).ConfigureAwait(false);
        }

        [Command("code")]
        [RequireModerator]
        [RequireContext(ContextType.Guild)]
        public async Task Code(IUser user)
        {
            var pendingData = await DB.PendingUsers.GetAsync(user.Id);

            if (pendingData is null)
            {
                await ReplyAsync("Этот пользователь не находится в списке ожидания на подтверждение.");
                return;
            }

            (var result, var code) =
                await riotService.GetThirdPartyCodeByEncryptedSummonerIdAsync(
                    pendingData.Region, pendingData.SummonedId);

            if (result != RequestResult.Success)
            {
                await ReplyAsync("Не удалось получить код подтверждения!");
                return;
            }

            await ReplyAsync($"Ожидаемый код: \"{pendingData.ConfirmationCode}\"\nВведённый код: \"{code}\"");
        }

        [Command("selftest")]
        [RequireDeveloper]
        public async Task SelfTest()
        {
            var skipChecks = false;

            var errors = new List<string>();
            var fixedRoles = 0u;

            var eb = new RiftEmbed().WithTitle("Self-test");

            if (!IonicClient.GetGuild(Settings.App.MainGuildId, out var guild))
            {
                errors.Add($"Guild is null: {nameof(Settings.App.MainGuildId)}");
                skipChecks = true;
            }

            var channelNames = Settings.ChannelId.GetNames();

            foreach (var field in Settings.ChannelId.GetType().GetProperties())
            {
                if (skipChecks)
                    break;

                if (field.GetValue(Settings.ChannelId, null) is ulong value)
                {
                    if (value == 0ul)
                    {
                        if (channelNames.ContainsKey(field.Name))
                        {
                            var channelName = channelNames[field.Name];

                            var guildChannel = guild.Channels.FirstOrDefault(
                                x => x.Name.Equals(channelName,
                                    StringComparison
                                        .InvariantCultureIgnoreCase));

                            if (guildChannel is null)
                            {
                                errors.Add($"Channel ID remains undefined: {field.Name} {channelName}");
                                continue;
                            }

                            Settings.ChannelId.SetValue(field.Name, guildChannel.Id);
                            fixedRoles++;
                        }
                        else
                        {
                            errors.Add($"Channel ID remains undefined: {field.Name}");
                            continue;
                        }
                    }
                    else if (!IonicClient.GetTextChannel(Settings.App.MainGuildId, value, out var textChannel) &&
                             !IonicClient.GetVoiceChannel(Settings.App.MainGuildId, value, out var voiceChannel))
                    {
                        errors.Add($"No channel on server: {field.Name}");
                    }
                }
            }

            foreach (var field in Settings.Chat.GetType().GetProperties())
            {
                if (skipChecks)
                    break;

                var obj = field.GetValue(Settings.Chat);

                if (obj is ulong ulongValue)
                {
                    if (ulongValue == 0ul) errors.Add($"Chat parameter undefined: {field.Name}");
                }
                else if (obj is uint uintValue)
                {
                    if (uintValue == 0u) errors.Add($"Chat parameter undefined: {field.Name}");
                }
            }

            foreach (var field in Settings.Economy.GetType().GetProperties())
            {
                if (skipChecks)
                    break;

                try
                {
                    var obj = field.GetValue(Settings.Economy);

                    if (obj is ulong ulongValue)
                    {
                        if (ulongValue == 0ul) errors.Add($"Economy parameter undefined: {field.Name}");
                    }
                    else if (obj is uint uintValue)
                    {
                        if (uintValue == 0u) errors.Add($"Economy parameter undefined: {field.Name}");
                    }
                }
                catch (TargetInvocationException ex)
                {
                    errors.Add($"\"{field.Name}\" invokation failed: {ex.Message}");
                }
                catch (Exception ex)
                {
                    errors.Add($"Economy object exception failed: {ex.Message}");
                }
            }

            var serverRoles = Context.Guild.Roles.ToList();
            var roles = await DB.Roles.GetAllAsync();

            foreach (var role in serverRoles)
            {
                if (skipChecks)
                    break;

                var matchedRole = roles.FirstOrDefault(x => x.Name.Equals(role.Name));

                if (matchedRole is null)
                {
                    await DB.Roles.AddAsync(role);
                    fixedRoles++;
                    continue;
                }

                if (matchedRole.RoleId.Equals(role.Id))
                    continue;

                matchedRole.RoleId = role.Id;
                await DB.Roles.UpdateAsync(matchedRole);
                fixedRoles++;
            }

            if (errors.Count == 0)
            {
                eb.WithColor(0, 255, 0);
                eb.WithDescription("OK 👌");
            }
            else
            {
                eb.WithColor(255, 0, 0);

                var errorList = string.Join('\n', errors);

                if (errorList.Length >= 2048)
                {
                    errorList = string.Join('\n', errors.Take(10));
                    eb.WithDescription($"**{errors.Count.ToString()} error(s), showing first 10**\n\n{errorList}");
                }
                else
                {
                    eb.WithDescription($"**{errors.Count.ToString()} error(s)**\n\n{errorList}");
                }
            }

            await Context.Channel.SendIonicMessageAsync(new IonicMessage(eb));

            if (fixedRoles > 0u)
            {
                var embedMsg = new RiftEmbed()
                    .WithColor(255, 255, 0)
                    .WithAuthor("Self-test")
                    .WithDescription($"Fixed {fixedRoles.ToString()} roles.");

                await Context.Channel.SendIonicMessageAsync(new IonicMessage(embedMsg));
            }
        }

        [Command("listroles")]
        [RequireAdmin]
        public async Task ListRoles()
        {
            var roles = new Queue<IRole>(Context.Guild.Roles);

            var amount = roles.Count;
            var page = 0;

            while (amount > 0)
            {
                ++page;

                var eb = new EmbedBuilder()
                    .WithAuthor($"Server roles")
                    .WithFooter($"Page {page.ToString()}");

                var ids = new List<ulong>();
                var names = new List<string>();

                var remove = amount >= 20 ? 20 : amount;

                for (var i = 0; i < remove; i++)
                {
                    var role = roles.Dequeue();

                    if (role == Context.Guild.EveryoneRole)
                        continue;

                    ids.Add(role.Id);
                    names.Add(role.Name);
                }

                eb.AddField("Role ID", string.Join('\n', ids), true);
                eb.AddField("Name", string.Join('\n', names), true);

                await Context.Channel.SendEmbedAsync(eb);

                amount -= remove;
            }
        }

        [Command("maintenance")]
        [RequireDeveloper]
        [RequireContext(ContextType.Guild)]
        public async Task Maintenance()
        {
            Settings.App.MaintenanceMode = !Settings.App.MaintenanceMode;

            if (Context.Client is DiscordSocketClient client) await client.SetGameAsync(RiftBot.BotStatusMessage);

            var message =
                await ReplyAsync($"Maintenance mode **{(Settings.App.MaintenanceMode ? "enabled" : "disabled")}**");
        }

        [Command("whois")]
        [RequireAdmin]
        [RequireContext(ContextType.Guild)]
        public async Task WhoIs(ulong userId)
        {
            var user = IonicClient.GetGuildUserById(Settings.App.MainGuildId, userId);

            if (user is null)
            {
                await ReplyAsync($"Пользователя с таким ID нет на сервере.");
                return;
            }

            await ReplyAsync($"Пользователь найден: {user.Mention} ({user.Username}#{user.Discriminator})");
        }

        [Command("ff")]
        [RequireAdmin]
        public async Task FF()
        {
            await Context.Message.DeleteAsync();
            IonicClient.TokenSource.Cancel();
        }

        [Command("reboot")]
        [RequireAdmin]
        public async Task Reboot()
        {
            RiftBot.ShouldReboot = true;

            await Context.Message.DeleteAsync();
            await ReplyAsync("Перезапускаюсь");

            IonicClient.TokenSource.Cancel();
        }

        [Command("listemotes")]
        [RequireAdmin]
        [RequireContext(ContextType.Guild)]
        public async Task Emotes()
        {
            var emotes = Context.Guild.Emotes.ToList();

            var re = new RiftEmbed()
                .WithAuthor("Server emotes")
                .AddField("Emote", string.Join('\n', emotes.Select(x => x.Name)), true)
                .AddField("ID", string.Join('\n', emotes.Select(x => x.Id)), true);

            await Context.Channel.SendIonicMessageAsync(new IonicMessage(re));
        }

        [Command("about")]
        [RequireModerator]
        [RequireContext(ContextType.Guild)]
        public async Task AppStatus()
        {
            var msg = await RiftBot.GetMessageAsync("bot-about", new FormatData(212997107525746690ul));

            if (msg is null)
                return;

            await Context.Channel.SendIonicMessageAsync(msg);
        }

        [Group("give")]
        public class GiveModule : ModuleBase
        {
            static async Task GiveAsync(IUser user, ItemReward reward)
            {
                if (!(user is SocketGuildUser sgUser))
                    return;

                await reward.DeliverToAsync(sgUser.Id);
                
                await RiftBot.SendMessageAsync("admin-give", Settings.ChannelId.Commands, new FormatData(sgUser.Id)
                {
                    Reward = reward
                });
            }

            [Command("coins")]
            [RequireAdmin]
            [RequireContext(ContextType.Guild)]
            public async Task Coins(uint amount, IUser user)
            {
                var reward = new ItemReward().AddCoins(amount);
                await GiveAsync(user, reward);
            }

            [Command("tokens")]
            [RequireAdmin]
            [RequireContext(ContextType.Guild)]
            public async Task Tokens(uint amount, IUser user)
            {
                var reward = new ItemReward().AddTokens(amount);
                await GiveAsync(user, reward);
            }

            [Command("chests")]
            [RequireAdmin]
            [RequireContext(ContextType.Guild)]
            public async Task Chests(uint amount, IUser user)
            {
                var reward = new ItemReward().AddChests(amount);
                await GiveAsync(user, reward);
            }

            [Command("capsules")]
            [RequireAdmin]
            [RequireContext(ContextType.Guild)]
            public async Task Capsules(uint amount, IUser user)
            {
                var reward = new ItemReward().AddCapsules(amount);
                await GiveAsync(user, reward);
            }

            [Command("spheres")]
            [RequireAdmin]
            [RequireContext(ContextType.Guild)]
            public async Task Spheres(uint amount, IUser user)
            {
                var reward = new ItemReward().AddSpheres(amount);
                await GiveAsync(user, reward);
            }

            [Command("2exp")]
            [RequireAdmin]
            [RequireContext(ContextType.Guild)]
            public async Task Levels(uint amount, IUser user)
            {
                var reward = new ItemReward().AddDoubleExps(amount);
                await GiveAsync(user, reward);
            }

            [Command("respects")]
            [RequireAdmin]
            [RequireContext(ContextType.Guild)]
            public async Task BotRespects(uint amount, IUser user)
            {
                var reward = new ItemReward().AddBotRespects(amount);
                await GiveAsync(user, reward);
            }

            [Command("tickets")]
            [RequireAdmin]
            [RequireContext(ContextType.Guild)]
            public async Task CustomTickets(uint amount, IUser user)
            {
                var reward = new ItemReward().AddTickets(amount);
                await GiveAsync(user, reward);
            }
        }

        [Group("take")]
        public class TakeModule : ModuleBase
        {
            async Task TakeAsync(IUser user, ItemReward reward)
            {
                if (!(user is SocketGuildUser sgUser))
                    return;

                var invData = reward.ToInventoryData();

                await DB.Inventory.RemoveAsync(sgUser.Id, invData);
                
                await RiftBot.SendMessageAsync("admin-take", Settings.ChannelId.Commands, new FormatData(sgUser.Id)
                {
                    Reward = reward
                });
            }
            
            [Command("coins")]
            [RequireAdmin]
            [RequireContext(ContextType.Guild)]
            public async Task Coins(uint amount, IUser user)
            {
                var reward = new ItemReward().AddCoins(amount);
                await TakeAsync(user, reward);
            }

            [Command("tokens")]
            [RequireAdmin]
            [RequireContext(ContextType.Guild)]
            public async Task Tokens(uint amount, IUser user)
            {
                var reward = new ItemReward().AddTokens(amount);
                await TakeAsync(user, reward);
            }

            [Command("2exp")]
            [RequireAdmin]
            [RequireContext(ContextType.Guild)]
            public async Task Level(uint amount, IUser user)
            {
                var reward = new ItemReward().AddDoubleExps(amount);
                await TakeAsync(user, reward);
            }

            [Command("chests")]
            [RequireAdmin]
            [RequireContext(ContextType.Guild)]
            public async Task Chests(uint amount, IUser user)
            {
                var reward = new ItemReward().AddChests(amount);
                await TakeAsync(user, reward);
            }

            [Command("capsules")]
            [RequireAdmin]
            [RequireContext(ContextType.Guild)]
            public async Task Capsules(uint amount, IUser user)
            {
                var reward = new ItemReward().AddCapsules(amount);
                await TakeAsync(user, reward);
            }

            [Command("spheres")]
            [RequireAdmin]
            [RequireContext(ContextType.Guild)]
            public async Task Spheres(uint amount, IUser user)
            {
                var reward = new ItemReward().AddSpheres(amount);
                await TakeAsync(user, reward);
            }

            [Command("respects")]
            [RequireAdmin]
            [RequireContext(ContextType.Guild)]
            public async Task BotRespects(uint amount, IUser user)
            {
                var reward = new ItemReward().AddBotRespects(amount);
                await TakeAsync(user, reward);
            }

            [Command("tickets")]
            [RequireAdmin]
            [RequireContext(ContextType.Guild)]
            public async Task CustomTickets(uint amount, IUser user)
            {
                var reward = new ItemReward().AddTickets(amount);
                await TakeAsync(user, reward);
            }
        }
    }
}
