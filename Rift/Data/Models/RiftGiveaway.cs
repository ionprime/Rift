﻿using System;
using System.Threading.Tasks;

using Humanizer;

namespace Rift.Data.Models
{
    public class RiftGiveaway
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public uint WinnersAmount { get; set; }
        public int StoredMessageId { get; set; }
        public int RewardId { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime CreatedAt { get; set; }
        public ulong CreatedBy { get; set; }

        public override string ToString()
        {
            var rewardData = Task.Run(async () => await DB.Rewards.GetAsync(RewardId)).Result;

            return $"{nameof(Name)}: {Name}\n" +
                   $"{nameof(Description)}: {Description}\n" +
                   $"{nameof(WinnersAmount)}: {WinnersAmount.ToString()}\n" +
                   $"{nameof(StoredMessageId)}: {StoredMessageId.ToString()}\n" +
                   $"{nameof(RewardId)}: {RewardId.ToString()}\n" +
                   $"RewardData: {rewardData.ToString()}\n" +
                   $"{nameof(Duration)}: {Duration.Humanize()}\n" +
                   $"{nameof(CreatedAt)}: {CreatedAt.Humanize()}\n" +
                   $"{nameof(CreatedBy)}: <@{CreatedBy.ToString()}>\n";
        }
    }
}
