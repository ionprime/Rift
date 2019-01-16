﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Rift.Configuration;
using Rift.Embeds;
using Rift.Preconditions;
using Rift.Services;

using IonicLib;
using IonicLib.Util;

using Discord;
using Discord.Commands;
using Discord.Webhook;
using Discord.WebSocket;

using Newtonsoft.Json;

namespace Rift.Modules
{
    public class RoleModule : RiftModuleBase
    {
        readonly RoleService roleService;

        public RoleModule(RoleService roleService)
        {
            this.roleService = roleService;
        }

        [Command("роли")]
        [RequireContext(ContextType.Guild)]
        public async Task Roles()
        {
            using (Context.Channel.EnterTypingState())
            {
                await Context.User.SendEmbedAsync(RoleEmbeds.Roles);
            }
        }

        [Command("стрим")]
        [RequireStreamer]
        [RequireContext(ContextType.Guild)]
        public async Task StreamStart([Remainder] string description)
        {
            var streamer = RiftBot.GetStreamer(Context.User.Id);

            if (streamer is null)
            {
                await Context.User.SendMessageAsync($"Вы не являетесь зарегистрированным стримером.");
                return;
            }

            var eb = new EmbedBuilder()
                     .WithAuthor("Трансляция")
                     .WithThumbnailUrl(streamer.PictureURL)
                     .WithDescription($"{description}")
                     .AddField($"Ссылка на трансляцию", streamer.StreamURL)
                     .Build();

            var webhook = streamer.Platform == RiftBot.StreamPlatform.Twitch
                ? new DiscordWebhookClient(505849754119569409ul,
                                           "NuoQqSufIqYo6_K2UF-NjU2N_-JfAACdLLKPOtqcquJStJLzTQF3OHcjLRcwEQybw4kR") // Twitch
                : new DiscordWebhookClient(505849783450206240ul,
                                           "woiEbSnQsiSV2QfHnkgakKWwoA2E_VQcc4rY_9HWhl1MMMBS1Of3vnFGqfF3sDuvCG3T"); // Youtube

            if (IonicClient.HasRolesAny(Context.Guild.Id, Context.User.Id, Settings.RoleId.Streamer))
                await
                    webhook.SendMessageAsync($"Стример {Context.User.Mention} запустил трансляцию, присоединяйтесь. @here",
                                             embeds: new Embed[]
                                             {
                                                 eb
                                             });
            else
                await webhook.SendMessageAsync($"{Context.User.Mention} онлайн!", embeds: new Embed[]
                {
                    eb
                });
        }

        [Group("роль")]
        public class SpecialRoleModule : ModuleBase
        {
            readonly EconomyService economyService;
            readonly RoleService roleService;
            readonly DatabaseService databaseService;

            public SpecialRoleModule(EconomyService economyService, RoleService roleService,
                                     DatabaseService databaseService)
            {
                this.economyService = economyService;
                this.roleService = roleService;
                this.databaseService = databaseService;
            }

            [Command("активные")]
            [RequireAdmin]
            [RequireContext(ContextType.Guild)]
            public async Task Active(IUser user)
            {
                if (!IonicClient.GetTextChannel(Settings.App.MainGuildId, Settings.ChannelId.Chat, out var chatChannel))
                    return;

                if (!(user is SocketGuildUser sgUser))
                    return;

                if (!IonicClient.GetRole(Settings.App.MainGuildId, Settings.RoleId.Active, out var role))
                {
                    await Context.User.SendMessageAsync($"Не найдена роль \"Активные\"");
                    return;
                }

                if (sgUser.Roles.Contains(role))
                {
                    await Context.User.SendMessageAsync($"У этого пользователя уже есть такая роль.");
                    return;
                }

                await sgUser.AddRoleAsync(role);

                var chatEmbed = new EmbedBuilder()
                                .WithAuthor("Оповещение")
                                .WithDescription($"Призывателю {sgUser.Mention} выдается роль **{role.Name.ToLowerInvariant()}**");

                await chatChannel.SendEmbedAsync(chatEmbed);
            }
        }
    }
}
