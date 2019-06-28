﻿using System.Threading.Tasks;

using Rift.Data.Models;

namespace Rift.Services.Message.Formatters.Inventory
{
    public class UserInventoryChests : FormatterBase
    {
        public UserInventoryChests() : base("$userInventoryChests") {}

        public override async Task<RiftMessage> Format(RiftMessage message, FormatData data)
        {
            var inventory = await DB.Inventory.GetAsync(data.UserId);
            return await ReplaceData(message, inventory.Chests.ToString());
        }
    }
}
