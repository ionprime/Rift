﻿using System.Linq;
using System.Threading.Tasks;

using Rift.Data;
using Rift.Data.Models;

using Microsoft.EntityFrameworkCore;

namespace Rift.Database
{
    public class LolData
    {
        public async Task AddAsync(RiftLolData lolData)
        {
            await DB.Users.EnsureExistsAsync(lolData.UserId);

            using (var context = new RiftContext())
            {
                await context.LolData.AddAsync(lolData);
                await context.SaveChangesAsync();
            }
        }

        public async Task<RiftLolData> GetAsync(ulong userId)
        {
            using (var context = new RiftContext())
            {
                return await context.LolData
                    .Where(x => x.UserId == userId)
                    .Select(x => new RiftLolData
                    {
                        UserId = x.UserId,
                        SummonerRegion = x.SummonerRegion,
                        PlayerUUID = x.PlayerUUID,
                        AccountId = x.AccountId,
                        SummonerId = x.SummonerId,
                        SummonerName = x.SummonerName,
                    })
                    .FirstOrDefaultAsync();
            }
        }

        public async Task UpdateAsync(ulong userId, string region, string playerUuid, string accountId, string summonerId, string summonerName)
        {
            var dbSummoner = await GetAsync(userId);

            var lolData = new RiftLolData
            {
                UserId = userId,
                SummonerRegion = region,
                PlayerUUID = playerUuid,
                SummonerId = summonerId,
                SummonerName = summonerName,
            };

            using (var context = new RiftContext())
            {
                var entity = context.Attach(lolData);

                if (dbSummoner?.SummonerRegion != region)
                    entity.Property(x => x.SummonerRegion).IsModified = true;

                if (dbSummoner?.PlayerUUID != playerUuid)
                    entity.Property(x => x.PlayerUUID).IsModified = true;

                if (dbSummoner?.SummonerId != summonerId)
                    entity.Property(x => x.SummonerId).IsModified = true;

                if (dbSummoner?.SummonerName != summonerName)
                    entity.Property(x => x.SummonerName).IsModified = true;

                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveAsync(ulong userId)
        {
            var lolData = new RiftLolData
            {
                UserId = userId,
            };

            using (var context = new RiftContext())
            {
                context.LolData.Remove(lolData);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasAsync(ulong userId)
        {
            var result = await GetAsync(userId);

            return !(result is null) && !string.IsNullOrWhiteSpace(result.PlayerUUID);
        }

        public async Task<bool> IsTakenAsync(string region, string playerUUID)
        {
            using (var context = new RiftContext())
            {
                return await context.LolData.AnyAsync(x => x.PlayerUUID == playerUUID && x.SummonerRegion == region);
            }
        }
    }
}
