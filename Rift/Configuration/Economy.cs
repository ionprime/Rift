﻿using System;

namespace Rift.Configuration
{
    public class Economy
    {
        public TimeSpan MessageCooldown { get; set; } = TimeSpan.FromSeconds(15);
        public TimeSpan GiftCooldown { get; set; } = TimeSpan.FromHours(1);
        public TimeSpan StoreCooldown { get; set; } = TimeSpan.FromMinutes(30);
        public TimeSpan AttackPerUserCooldown { get; set; } = TimeSpan.FromHours(2);
        public TimeSpan AttackSameUserCooldown { get; set; } = TimeSpan.FromHours(4);
        public TimeSpan BragCooldownSeconds { get; set; } = TimeSpan.FromHours(6);
        public uint AttackPrice { get; set; } = 0u;
        public uint AttackMinimumLevel { get; set; } = 0u;
        public int BragWinCoinsMin { get; set; } = 0;
        public int BragWinCoinsMax { get; set; } = 0;
        public int BragLossCoinsMin { get; set; } = 0;
        public int BragLossCoinsMax { get; set; } = 0;
        public TimeSpan PendingUserLifeTime { get; set; } = TimeSpan.FromHours(3);
        public TimeSpan LolAccountUpdateCooldown { get; set; } = TimeSpan.FromHours(6);
    }
}
