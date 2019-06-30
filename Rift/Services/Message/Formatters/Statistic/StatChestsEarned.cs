﻿using System.Threading.Tasks;

using Rift.Data.Models;

namespace Rift.Services.Message.Formatters.Statistic
{
    public class StatChestsEarned : FormatterBase
    {
        public StatChestsEarned() : base("$statChestsEarned") {}

        public override async Task<RiftMessage> Format(RiftMessage message, FormatData data)
        {
            return await ReplaceData(message, data.Statistics.ChestsEarned.ToString());
        }
    }
}