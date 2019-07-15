﻿using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Rift.Services.Reward
{
    public abstract class RewardBase
    {
        [JsonIgnore] protected RewardType Type;
        public abstract Task DeliverToAsync(ulong userId);
    }

    public enum RewardType
    {
        Undefined = 0,
        Item = 1,
        Role = 2,
        Background = 3,
    }
}
