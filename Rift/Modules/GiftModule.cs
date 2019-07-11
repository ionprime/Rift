﻿using System.Threading.Tasks;

using Rift.Services;
using Rift.Util;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Rift.Modules
{
    public class GiftModule : RiftModuleBase
    {
        readonly GiftService giftService;

        public GiftModule(GiftService giftService)
        {
            this.giftService = giftService;
        }

        [Command("подарки")]
        [RequireContext(ContextType.Guild)]
        public async Task Gifts()
        {
            await giftService.SendDescriptionAsync();
        }

        [Command("подарить")]
        [RequireContext(ContextType.Guild)]
        public async Task Default(IUser user)
        {
            using (Context.Channel.EnterTypingState())
            {
                if (user is null || !(user is SocketGuildUser sgUser))
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} пользователь не найден!");
                    return;
                }

                var message = await giftService.SendGiftAsync((SocketGuildUser) Context.User, sgUser);

                if (message is null)
                    return;

                await Context.Channel.SendIonicMessageAsync(message);
            }
        }
    }
}
