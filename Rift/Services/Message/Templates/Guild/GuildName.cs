﻿using System.Threading.Tasks;

using Rift.Configuration;
using Rift.Data.Models;

using IonicLib;

namespace Rift.Services.Message.Templates.Guild
{
    public class GuildName : TemplateBase
    {
        public GuildName() : base(nameof(GuildName)) {}

        public override Task<RiftMessage> Apply(RiftMessage message, FormatData data)
        {
            if (!IonicClient.GetGuild(Settings.App.MainGuildId, out var guild))
            {
                TemplateError($"No guild found.");
                return Task.FromResult(message);
            }

            return ReplaceData(message, guild.Name);
        }
    }
}