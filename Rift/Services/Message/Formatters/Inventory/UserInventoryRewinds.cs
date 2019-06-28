﻿using System.Threading.Tasks;

using Rift.Data.Models;

namespace Rift.Services.Message.Formatters.Inventory
{
    public class UserInventoryRewinds : FormatterBase
    {
        public UserInventoryRewinds() : base("$userInventoryRewinds") {}

        public override async Task<RiftMessage> Format(RiftMessage message, FormatData data)
        {
            var inventory = await DB.Inventory.GetAsync(data.UserId);
            return await ReplaceData(message, inventory.BonusRewind.ToString());
        }
    }
}
