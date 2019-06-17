﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Rift.Data;

namespace Rift.Migrations
{
    [DbContext(typeof(RiftContext))]
    [Migration("20190617181905_AddedStatVoiceUptime")]
    partial class AddedStatVoiceUptime
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Rift.Data.Models.RiftCooldowns", b =>
                {
                    b.Property<ulong>("UserId");

                    b.Property<DateTime>("BotRespectTime");

                    b.Property<DateTime>("DoubleExpTime");

                    b.Property<DateTime>("LastBackgroundStoreTime");

                    b.Property<DateTime>("LastBragTime");

                    b.Property<DateTime>("LastDailyChestTime");

                    b.Property<DateTime>("LastGiftTime");

                    b.Property<DateTime>("LastLolAccountUpdateTime");

                    b.Property<DateTime>("LastRoleStoreTime");

                    b.Property<DateTime>("LastStoreTime");

                    b.HasKey("UserId");

                    b.ToTable("Cooldowns");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftGiveaway", b =>
                {
                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<ulong>("CreatedBy");

                    b.Property<string>("Description");

                    b.Property<TimeSpan>("Duration");

                    b.Property<int>("RewardId");

                    b.Property<int>("StoredMessageId");

                    b.Property<uint>("WinnersAmount");

                    b.HasKey("Name");

                    b.ToTable("Giveaways");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftGiveawayActive", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("ChannelMessageId");

                    b.Property<DateTime>("DueTime");

                    b.Property<string>("GiveawayName");

                    b.Property<DateTime>("StartedAt");

                    b.Property<ulong>("StartedBy");

                    b.Property<int>("StoredMessageId");

                    b.HasKey("Id");

                    b.ToTable("ActiveGiveaways");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftGiveawayLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<TimeSpan>("Duration");

                    b.Property<DateTime>("FinishedAt");

                    b.Property<string>("Name");

                    b.Property<string>("ParticipantsString");

                    b.Property<string>("Reward");

                    b.Property<DateTime>("StartedAt");

                    b.Property<ulong>("StartedBy");

                    b.Property<string>("WinnersString");

                    b.HasKey("Id");

                    b.ToTable("GiveawayLogs");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftInventory", b =>
                {
                    b.Property<ulong>("UserId");

                    b.Property<uint>("BonusBotRespect");

                    b.Property<uint>("BonusDoubleExp");

                    b.Property<uint>("BonusRewind");

                    b.Property<uint>("Capsules");

                    b.Property<uint>("Chests");

                    b.Property<uint>("Coins");

                    b.Property<uint>("Essence");

                    b.Property<uint>("Spheres");

                    b.Property<uint>("Tickets");

                    b.Property<uint>("Tokens");

                    b.HasKey("UserId");

                    b.ToTable("Inventory");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftLolData", b =>
                {
                    b.Property<ulong>("UserId");

                    b.Property<string>("AccountId");

                    b.Property<string>("PlayerUUID");

                    b.Property<string>("SummonerId");

                    b.Property<string>("SummonerName");

                    b.Property<string>("SummonerRegion");

                    b.HasKey("UserId");

                    b.ToTable("LolData");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftMapping", b =>
                {
                    b.Property<string>("Identifier")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("MessageId");

                    b.HasKey("Identifier");

                    b.ToTable("MessageMappings");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("ApplyFormat");

                    b.Property<string>("Embed");

                    b.Property<string>("ImageUrl");

                    b.Property<string>("Name");

                    b.Property<string>("Text");

                    b.HasKey("Id");

                    b.ToTable("StoredMessages");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftModerationLog", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Action");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<TimeSpan>("Duration");

                    b.Property<ulong>("ModeratorId");

                    b.Property<string>("Reason");

                    b.Property<ulong>("TargetId");

                    b.HasKey("Id");

                    b.ToTable("ModerationLog");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftPendingUser", b =>
                {
                    b.Property<ulong>("UserId");

                    b.Property<string>("AccountId");

                    b.Property<string>("ConfirmationCode");

                    b.Property<DateTime>("ExpirationTime");

                    b.Property<string>("PlayerUUID");

                    b.Property<string>("Region");

                    b.Property<string>("SummonedId");

                    b.HasKey("UserId");

                    b.ToTable("PendingUsers");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftReward", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("ItemsData");

                    b.Property<string>("RoleData");

                    b.HasKey("Id");

                    b.ToTable("Rewards");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftRoleInventory", b =>
                {
                    b.Property<ulong>("UserId");

                    b.Property<ulong>("RoleId");

                    b.Property<DateTime>("ObtainedAt");

                    b.Property<string>("ObtainedFrom");

                    b.HasKey("UserId", "RoleId");

                    b.ToTable("RoleInventories");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftSettings", b =>
                {
                    b.Property<int>("Id");

                    b.Property<string>("Data");

                    b.HasKey("Id");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftStatistics", b =>
                {
                    b.Property<ulong>("UserId");

                    b.Property<uint>("AttacksDone");

                    b.Property<uint>("AttacksReceived");

                    b.Property<uint>("BragTotal");

                    b.Property<uint>("CapsuleEarnedTotal");

                    b.Property<uint>("CapsuleOpenedTotal");

                    b.Property<uint>("ChestsEarnedTotal");

                    b.Property<uint>("ChestsOpenedTotal");

                    b.Property<uint>("CoinsEarnedTotal");

                    b.Property<uint>("CoinsSpentTotal");

                    b.Property<uint>("EssenceEarnedTotal");

                    b.Property<uint>("GiftsReceived");

                    b.Property<uint>("GiftsSent");

                    b.Property<uint>("MessagesSentTotal");

                    b.Property<uint>("PurchasedItemsTotal");

                    b.Property<uint>("SphereEarnedTotal");

                    b.Property<uint>("SphereOpenedTotal");

                    b.Property<uint>("TokensEarnedTotal");

                    b.Property<uint>("TokensSpentTotal");

                    b.Property<TimeSpan>("VoiceUptime");

                    b.HasKey("UserId");

                    b.ToTable("Statistics");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftStreamer", b =>
                {
                    b.Property<ulong>("UserId");

                    b.Property<string>("PictureUrl");

                    b.Property<string>("StreamUrl");

                    b.HasKey("UserId");

                    b.ToTable("Streamers");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftSystemTimer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<TimeSpan>("Interval");

                    b.Property<DateTime>("LastInvoked");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("SystemCooldowns");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftTempRole", b =>
                {
                    b.Property<ulong>("UserId");

                    b.Property<ulong>("RoleId");

                    b.Property<DateTime>("ExpirationTime");

                    b.Property<string>("ObtainedFrom");

                    b.Property<DateTime>("ObtainedTime");

                    b.HasKey("UserId", "RoleId");

                    b.ToTable("TempRoles");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftToxicity", b =>
                {
                    b.Property<ulong>("UserId");

                    b.Property<DateTime>("LastDecreased");

                    b.Property<DateTime>("LastIncreased");

                    b.Property<uint>("Percent");

                    b.HasKey("UserId");

                    b.ToTable("Toxicity");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftUser", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<uint>("Experience");

                    b.Property<uint>("Level");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Rift.Data.Models.ScheduledEvent", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DayId");

                    b.Property<int>("EventId");

                    b.Property<int>("Hour");

                    b.Property<int>("Minute");

                    b.HasKey("Id");

                    b.ToTable("ScheduledEvents");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftCooldowns", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithOne("Cooldowns")
                        .HasForeignKey("Rift.Data.Models.RiftCooldowns", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rift.Data.Models.RiftInventory", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithOne("Inventory")
                        .HasForeignKey("Rift.Data.Models.RiftInventory", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rift.Data.Models.RiftLolData", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithOne("LolData")
                        .HasForeignKey("Rift.Data.Models.RiftLolData", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rift.Data.Models.RiftPendingUser", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithOne("PendingUser")
                        .HasForeignKey("Rift.Data.Models.RiftPendingUser", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rift.Data.Models.RiftRoleInventory", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rift.Data.Models.RiftStatistics", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithOne("Statistics")
                        .HasForeignKey("Rift.Data.Models.RiftStatistics", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rift.Data.Models.RiftStreamer", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithOne("Streamers")
                        .HasForeignKey("Rift.Data.Models.RiftStreamer", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rift.Data.Models.RiftTempRole", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithMany("TempRoles")
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_RiftTempRoles_Users_UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Rift.Data.Models.RiftToxicity", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithOne("Toxicity")
                        .HasForeignKey("Rift.Data.Models.RiftToxicity", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
