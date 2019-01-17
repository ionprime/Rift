using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Rift.Configuration;
using Rift.Data.Models;
using Rift.Embeds;

using IonicLib;

using Discord;
using Discord.WebSocket;

using Humanizer;

namespace Rift.Services
{
    public class RoleService
    {
        static Timer tempRoleTimer;

        public RoleService()
        {
            tempRoleTimer = new Timer(async delegate { await TimerProcAsync(); }, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
        }

        async Task TimerProcAsync()
        {
            var expiredRoles = await RiftBot.GetService<DatabaseService>().GetExpiredTempRolesAsync();

            if (expiredRoles is null || expiredRoles.Count == 0)
                return;

            foreach (var expiredRole in expiredRoles)
            {
                await RemoveTempRoleAsync(expiredRole.UserId, expiredRole.RoleId);
            }
        }

        public async Task<(bool, Embed)> AddPermanentRoleAsync(ulong userId, ulong roleId)
        {
            var sgUser = IonicClient.GetGuildUserById(Settings.App.MainGuildId, userId);

            if (sgUser is null)
                return (false, GenericEmbeds.UserNotFound);

            if (!IonicClient.GetRole(Settings.App.MainGuildId, roleId, out var role))
                return (false, GenericEmbeds.RoleNotFound);

            await sgUser.AddRoleAsync(role);
            return (true, null);
        }

        public async Task AddTempRoleAsync(ulong userId, ulong roleId, TimeSpan duration, string reason)
        {
            var role = new RiftTempRole
            {
                UserId = userId,
                RoleId = roleId,
                ObtainedFrom = reason,
                ObtainedTime = DateTime.UtcNow,
                ExpirationTime = DateTime.UtcNow + duration,
            };

            var sgUser = IonicClient.GetGuildUserById(Settings.App.MainGuildId, userId);

            if (sgUser is null)
                return;

            if (!IonicClient.GetRole(Settings.App.MainGuildId, roleId, out var serverRole))
                return;

            await sgUser.AddRoleAsync(serverRole);

            await RiftBot.GetService<DatabaseService>()
                         .AddTempRoleAsync(role);
        }

        public async Task<(bool, Embed)> RemoveTempRoleAsync(ulong userId, ulong roleId)
        {
            var sgUser = IonicClient.GetGuildUserById(Settings.App.MainGuildId, userId);

            if (sgUser is null)
                return (false, GenericEmbeds.UserNotFound);

            await RiftBot.GetService<DatabaseService>().RemoveUserTempRoleAsync(userId, roleId);

            var role = sgUser.Roles.FirstOrDefault(x => x.Id == roleId);

            if (role != null)
                await sgUser.RemoveRoleAsync(role);

            RiftBot.Log.Info($"Removed role {roleId} from {sgUser} {userId.ToString()}");

            return (true, null);
        }

        public async Task<List<RiftTempRole>> GetUserTempRolesAsync(ulong userId)
        {
            return await RiftBot.GetService<DatabaseService>().GetUserTempRolesAsync(userId);
        }

        public async Task RestoreTempRolesAsync(SocketGuildUser sgUser)
        {
            RiftBot.Log.Info($"User {sgUser} ({sgUser.Id}) joined, checking temp roles");

            var tempRoles = await RiftBot.GetService<DatabaseService>().GetUserTempRolesAsync(sgUser.Id);

            if (tempRoles is null)
            {
                RiftBot.Log.Debug($"No temp roles for user {sgUser}");
                return;
            }

            foreach (var tempRole in tempRoles)
            {
                if (sgUser.Roles.Any(x => x.Id == tempRole.RoleId))
                    continue;

                if (!IonicClient.GetRole(Settings.App.MainGuildId, tempRole.RoleId, out var role))
                {
                    RiftBot.Log.Error($"Applying role {tempRole.RoleId}: FAILED");
                    continue;
                }

                await sgUser.AddRoleAsync(role);
                RiftBot.Log.Debug($"Successfully added temp role \"{role.Name}\" for user {sgUser}");
            }
        }
    }
}
