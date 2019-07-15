﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Rift.Data;
using Rift.Data.Models;

using Microsoft.EntityFrameworkCore;

namespace Rift.Database
{
    public class Quests
    {
        public async Task AddQuestProgressAsync(ulong userId, RiftQuestProgress userQuest)
        {
            var dbUserQuest = await GetQuestProgressAsync(userId, userQuest.QuestId);

            if (!(dbUserQuest is null))
                return;

            using (var context = new RiftContext())
            {
                await context.QuestProgress.AddAsync(userQuest);
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<StageData>> GetAllStagedQuestsAsync()
        {
            using (var context = new RiftContext())
            {
                var dbStages = await context.QuestStages.ToListAsync();

                if (dbStages is null || dbStages.Count == 0)
                    return null;

                var stages = new List<StageData>();

                foreach (var dbStage in dbStages)
                    stages.Add(new StageData
                    {
                        Stage = dbStage,
                        Quests = await GetStageQuestsAsync(dbStage.Id)
                    });

                return stages;
            }
        }

        public async Task<List<RiftQuest>> GetStageQuestsAsync(int stageId)
        {
            using (var context = new RiftContext())
            {
                return await context.Quests
                                    .Where(x => x.StageId == stageId)
                                    .OrderBy(x => x.Order)
                                    .ToListAsync();
            }
        }

        public async Task<List<RiftQuest>> GetAllQuestsAsync()
        {
            using (var context = new RiftContext())
            {
                return await context.Quests
                                    .OrderBy(x => x.StageId)
                                    .ThenBy(x => x.Order)
                                    .ToListAsync();
            }
        }

        public async Task<RiftQuestProgress> GetQuestProgressAsync(ulong userId, int questId)
        {
            using (var context = new RiftContext())
            {
                return await context.QuestProgress
                                    .FirstOrDefaultAsync(x => x.UserId == userId && x.QuestId == questId);
            }
        }

        public async Task<List<RiftQuestProgress>> GetActiveQuestsProgressAsync(ulong userId)
        {
            using (var context = new RiftContext())
            {
                return await context.QuestProgress
                                    .Where(x => x.UserId == userId && !x.IsCompleted)
                                    .ToListAsync();
            }
        }

        public async Task<RiftQuest> GetQuestAsync(int id)
        {
            using (var context = new RiftContext())
            {
                return await context.Quests
                                    .FirstOrDefaultAsync(x => x.Id == id);
            }
        }

        public async Task SetQuestsProgressAsync(RiftQuestProgress userQuest)
        {
            var dbUserQuest = await GetQuestProgressAsync(userQuest.UserId, userQuest.QuestId);

            if (dbUserQuest is null)
            {
                await AddQuestProgressAsync(userQuest.UserId, userQuest);
                return;
            }

            using (var context = new RiftContext())
            {
                var entry = context.Entry(userQuest);

                if (dbUserQuest.IsCompleted != userQuest.IsCompleted)
                    entry.Property(x => x.IsCompleted).IsModified = true;

                if (dbUserQuest.ApprovedLolAccount != userQuest.ApprovedLolAccount)
                    entry.Property(x => x.ApprovedLolAccount).IsModified = true;

                if (dbUserQuest.BragsDone != userQuest.BragsDone)
                    entry.Property(x => x.BragsDone).IsModified = true;

                if (dbUserQuest.MessagesSent != userQuest.MessagesSent)
                    entry.Property(x => x.MessagesSent).IsModified = true;

                if (dbUserQuest.BoughtChests != userQuest.BoughtChests)
                    entry.Property(x => x.BoughtChests).IsModified = true;

                if (dbUserQuest.OpenedChests != userQuest.OpenedChests)
                    entry.Property(x => x.OpenedChests).IsModified = true;

                if (dbUserQuest.NormalMonstersKilled != userQuest.NormalMonstersKilled)
                    entry.Property(x => x.NormalMonstersKilled).IsModified = true;

                if (dbUserQuest.RareMonstersKilled != userQuest.RareMonstersKilled)
                    entry.Property(x => x.RareMonstersKilled).IsModified = true;

                if (dbUserQuest.EpicMonstersKilled != userQuest.EpicMonstersKilled)
                    entry.Property(x => x.EpicMonstersKilled).IsModified = true;

                if (dbUserQuest.GiftsSent != userQuest.GiftsSent)
                    entry.Property(x => x.GiftsSent).IsModified = true;

                if (dbUserQuest.GiftsReceived != userQuest.GiftsReceived)
                    entry.Property(x => x.GiftsReceived).IsModified = true;

                if (dbUserQuest.GiftsReceivedFromUltraGay != userQuest.GiftsReceivedFromUltraGay)
                    entry.Property(x => x.GiftsReceivedFromUltraGay).IsModified = true;

                if (dbUserQuest.LevelReached != userQuest.LevelReached)
                    entry.Property(x => x.LevelReached).IsModified = true;

                if (dbUserQuest.GiveawaysParticipated != userQuest.GiveawaysParticipated)
                    entry.Property(x => x.GiveawaysParticipated).IsModified = true;

                if (dbUserQuest.CoinsReceived != userQuest.CoinsReceived)
                    entry.Property(x => x.CoinsReceived).IsModified = true;

                if (dbUserQuest.CoinsSpent != userQuest.CoinsSpent)
                    entry.Property(x => x.CoinsSpent).IsModified = true;

                if (dbUserQuest.VoiceUptimeEarned != userQuest.VoiceUptimeEarned)
                    entry.Property(x => x.VoiceUptimeEarned).IsModified = true;

                if (dbUserQuest.GiftedBotKeeper != userQuest.GiftedBotKeeper)
                    entry.Property(x => x.GiftedBotKeeper).IsModified = true;

                if (dbUserQuest.GiftedModerator != userQuest.GiftedModerator)
                    entry.Property(x => x.GiftedModerator).IsModified = true;

                if (dbUserQuest.GiftedStreamer != userQuest.GiftedStreamer)
                    entry.Property(x => x.GiftedStreamer).IsModified = true;

                if (dbUserQuest.ActivatedBotRespects != userQuest.ActivatedBotRespects)
                    entry.Property(x => x.ActivatedBotRespects).IsModified = true;

                if (dbUserQuest.OpenedSphere != userQuest.OpenedSphere)
                    entry.Property(x => x.OpenedSphere).IsModified = true;

                if (dbUserQuest.RolesPurchased != userQuest.RolesPurchased)
                    entry.Property(x => x.RolesPurchased).IsModified = true;

                await context.SaveChangesAsync();
            }
        }
    }

    public class StageData
    {
        public RiftStage Stage { get; set; }
        public List<RiftQuest> Quests { get; set; }
    }
}
