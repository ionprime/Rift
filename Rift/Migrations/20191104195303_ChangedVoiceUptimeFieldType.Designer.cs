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
    [Migration("20191104195303_ChangedVoiceUptimeFieldType")]
    partial class ChangedVoiceUptimeFieldType
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Rift.Data.Models.RiftActiveEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<ulong>("ChannelMessageId")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTime>("DueTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("EventName")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("StartedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("StartedBy")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("StoredMessageId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("ActiveEvents");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftActiveGiveaway", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<ulong>("ChannelMessageId")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTime>("DueTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("GiveawayName")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("StartedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("StartedBy")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("StoredMessageId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("ActiveGiveaways");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftBackgroundInventory", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("BackgroundId")
                        .HasColumnType("int");

                    b.HasKey("UserId", "BackgroundId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("BackgroundInventories");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftCooldowns", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTime>("BotRespectTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("DoubleExpTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("LastBackgroundStoreTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("LastBragTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("LastCommunityVoteTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("LastDailyRewardTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("LastGiftTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("LastItemStoreTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("LastLolAccountUpdateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("LastRoleStoreTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("LastStreamerVoteTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("LastTeamVoteTime")
                        .HasColumnType("datetime(6)");

                    b.HasKey("UserId");

                    b.ToTable("Cooldowns");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftEvent", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("CreatedBy")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("time(6)");

                    b.Property<int>("SharedRewardId")
                        .HasColumnType("int");

                    b.Property<int>("SpecialRewardId")
                        .HasColumnType("int");

                    b.Property<int>("StoredMessageId")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Name");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftEventLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("time(6)");

                    b.Property<DateTime>("FinishedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<uint>("ParticipantsAmount")
                        .HasColumnType("int unsigned");

                    b.Property<string>("Reward")
                        .HasColumnType("longtext");

                    b.Property<ulong>("SpecialWinnerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTime>("StartedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("StartedBy")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("EventLogs");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftGiveaway", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("CreatedBy")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("time(6)");

                    b.Property<int>("RewardId")
                        .HasColumnType("int");

                    b.Property<int>("StoredMessageId")
                        .HasColumnType("int");

                    b.Property<uint>("WinnersAmount")
                        .HasColumnType("int unsigned");

                    b.HasKey("Name");

                    b.ToTable("Giveaways");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftGiveawayLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("time(6)");

                    b.Property<DateTime>("FinishedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<string>("ParticipantsString")
                        .HasColumnType("longtext");

                    b.Property<string>("Reward")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("StartedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("StartedBy")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("WinnersString")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("GiveawayLogs");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftInventory", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<uint>("BonusBotRespect")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("BonusDoubleExp")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("BonusRewind")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("Capsules")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("Chests")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("Coins")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("Essence")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("Spheres")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("Tickets")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("Tokens")
                        .HasColumnType("int unsigned");

                    b.HasKey("UserId");

                    b.ToTable("Inventory");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftLeagueData", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("AccountId")
                        .HasColumnType("longtext");

                    b.Property<string>("PlayerUUID")
                        .HasColumnType("longtext");

                    b.Property<string>("SummonerId")
                        .HasColumnType("longtext");

                    b.Property<string>("SummonerName")
                        .HasColumnType("longtext");

                    b.Property<string>("SummonerRegion")
                        .HasColumnType("longtext");

                    b.HasKey("UserId");

                    b.ToTable("LeagueData");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftMapping", b =>
                {
                    b.Property<string>("Identifier")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("MessageId")
                        .HasColumnType("int");

                    b.HasKey("Identifier");

                    b.ToTable("MessageMappings");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Embed")
                        .HasColumnType("longtext");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<string>("Text")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftModerationLog", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    b.Property<string>("Action")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("time(6)");

                    b.Property<ulong>("ModeratorId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Reason")
                        .HasColumnType("longtext");

                    b.Property<ulong>("TargetId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("ModerationLog");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftPendingUser", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("AccountId")
                        .HasColumnType("longtext");

                    b.Property<string>("ConfirmationCode")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("ExpirationTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("PlayerUUID")
                        .HasColumnType("longtext");

                    b.Property<string>("Region")
                        .HasColumnType("longtext");

                    b.Property<string>("SummonedId")
                        .HasColumnType("longtext");

                    b.HasKey("UserId");

                    b.ToTable("PendingUsers");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftProfileBackground", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<string>("Url")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Backgrounds");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftReward", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("ItemsData")
                        .HasColumnType("longtext");

                    b.Property<string>("RoleData")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Rewards");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftRoleInventory", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.Property<DateTime>("ObtainedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ObtainedFrom")
                        .HasColumnType("longtext");

                    b.HasKey("UserId", "RoleId");

                    b.ToTable("RoleInventories");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftScheduledEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("EventType")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartAt")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.ToTable("EventSchedule");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftSettings", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Data")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftStatistics", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<uint>("BotRespectsActivated")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("BotRespectsEarned")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("BragsDone")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("CapsulesEarned")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("CapsulesOpened")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("ChestsEarned")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("ChestsOpened")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("CoinsEarned")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("CoinsSpent")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("DoubleExpsActivated")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("DoubleExpsEarned")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("EssenceEarned")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("EssenceSpent")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("GiftsReceived")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("GiftsSent")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("MessagesSent")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("PurchasedItems")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("RewindsActivated")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("RewindsEarned")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("SpheresEarned")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("SpheresOpened")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("TicketsEarned")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("TicketsSpent")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("TokensEarned")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("TokensSpent")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("VoiceUptimeHours")
                        .HasColumnType("int unsigned");

                    b.HasKey("UserId");

                    b.ToTable("Statistics");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftStreamer", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("BackgroundId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<string>("PictureUrl")
                        .HasColumnType("longtext");

                    b.Property<string>("StreamUrl")
                        .HasColumnType("longtext");

                    b.HasKey("UserId");

                    b.ToTable("Streamers");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftSystemTimer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<TimeSpan>("Interval")
                        .HasColumnType("time(6)");

                    b.Property<DateTime>("LastInvoked")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("SystemCooldowns");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftTempRole", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTime>("ExpirationTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ObtainedFrom")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("ObtainedTime")
                        .HasColumnType("datetime(6)");

                    b.HasKey("UserId", "RoleId");

                    b.ToTable("TempRoles");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftToxicity", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<DateTime>("LastDecreased")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("LastIncreased")
                        .HasColumnType("datetime(6)");

                    b.Property<uint>("Percent")
                        .HasColumnType("int unsigned");

                    b.HasKey("UserId");

                    b.ToTable("Toxicity");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftUser", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<uint>("Experience")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("Level")
                        .HasColumnType("int unsigned");

                    b.Property<int>("ProfileBackground")
                        .HasColumnType("int");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Rift.Data.Models.RiftBackgroundInventory", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithOne("BackgroundInventory")
                        .HasForeignKey("Rift.Data.Models.RiftBackgroundInventory", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Rift.Data.Models.RiftCooldowns", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithOne("Cooldowns")
                        .HasForeignKey("Rift.Data.Models.RiftCooldowns", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Rift.Data.Models.RiftInventory", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithOne("Inventory")
                        .HasForeignKey("Rift.Data.Models.RiftInventory", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Rift.Data.Models.RiftLeagueData", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithOne("LolData")
                        .HasForeignKey("Rift.Data.Models.RiftLeagueData", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Rift.Data.Models.RiftPendingUser", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithOne("PendingUser")
                        .HasForeignKey("Rift.Data.Models.RiftPendingUser", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Rift.Data.Models.RiftRoleInventory", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Rift.Data.Models.RiftStatistics", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithOne("Statistics")
                        .HasForeignKey("Rift.Data.Models.RiftStatistics", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Rift.Data.Models.RiftStreamer", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithOne("Streamers")
                        .HasForeignKey("Rift.Data.Models.RiftStreamer", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Rift.Data.Models.RiftTempRole", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithMany("TempRoles")
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_RiftTempRoles_Users_UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Rift.Data.Models.RiftToxicity", b =>
                {
                    b.HasOne("Rift.Data.Models.RiftUser", "User")
                        .WithOne("Toxicity")
                        .HasForeignKey("Rift.Data.Models.RiftToxicity", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}