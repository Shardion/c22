using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;
using Shardion.Achromatic.Configuration;
using LiteDB;
using Shardion.Achromatic.Features.Anomaly.Anomalies;
using System;
using Shardion.Achromatic.Common.Timers;
using Disqord.Bot.Hosting;
using Microsoft.Extensions.Logging;
using Shardion.Achromatic.Extensions;
using System.Collections.Generic;

namespace Shardion.Achromatic.Features.Anomaly.Shop
{
    public class AnomalyDatabaseService : DiscordBotService
    {
        public static readonly int REWARDS = 3;

        private readonly AnomalyOptions _options;
        public LiteDatabase Database { get; set; }

        public AnomalyDatabaseService(OptionsMultiplexer options, LiteDatabase db)
        {
            Database = db;
            _options = options.Get<AnomalyOptions>(OptionsAccessibility.NoOne, null, null) ?? new();
        }

        public int GetZebras(IMember member)
        {
            ILiteCollection<AnomalyRecord> anomalyRecords = Database.GetCollection<AnomalyRecord>("anomaly");
            AnomalyRecord memberRecord = FindOrCreateRecord(anomalyRecords, member);
            return memberRecord.Zebras;
        }

        public async Task SetZebras(IMember member, int zebras, bool grantRewards = true)
        {
            ILiteCollection<AnomalyRecord> anomalyRecords = Database.GetCollection<AnomalyRecord>("anomaly");
            AnomalyRecord memberRecord = FindOrCreateRecord(anomalyRecords, member);
            memberRecord.Zebras = zebras;
            anomalyRecords.Update(memberRecord);
            if (grantRewards)
            {
                await GrantZebraRewards(member);
            }
        }

        public async Task AddZebras(IMember member, int zebras, bool grantRewards = true)
        {
            ILiteCollection<AnomalyRecord> anomalyRecords = Database.GetCollection<AnomalyRecord>("anomaly");
            AnomalyRecord memberRecord = FindOrCreateRecord(anomalyRecords, member);
            memberRecord.Zebras += zebras;
            anomalyRecords.Update(memberRecord);
            if (grantRewards)
            {
                await GrantZebraRewards(member);
            }
        }

        private async Task GrantZebraRewards(IMember member)
        {
            ILiteCollection<AnomalyRecord> anomalyRecords = Database.GetCollection<AnomalyRecord>("anomaly");
            AnomalyRecord record = FindOrCreateRecord(anomalyRecords, member);
            Task<IChannel?> channelTask = Bot.GetOrFetchChannel(_options.AnnouncementGuild, _options.AnnouncementChannel);
            string? rewardText = null;
            if (record.Zebras >= 5 && !record.EarnedFirstRole)
            {
                record.EarnedFirstRole = true;
                anomalyRecords.Update(record);
                rewardText = $"{member.Mention} has earned the `🦓️` role!";
                await member.GrantRoleAsync(_options.FirstRoleRewardId);
            }
            if (record.Zebras >= 10 && record.Anomaly == AnomalyType.None)
            {
                await BecomeAnomaly(member, null);
            }
            if (record.Zebras >= 20 && !record.EarnedSecondRole)
            {
                // acommodate for becoming anomaly possibly screwing with the record
                record = FindOrCreateRecord(anomalyRecords, member);
                record.EarnedSecondRole = true;
                anomalyRecords.Update(record);
                rewardText = $"{member.Mention} has earned the `access to screm` role!";
                await member.GrantRoleAsync(_options.SecondRoleRewardId);
            }
            if (rewardText is not null)
            {
                if (await channelTask is IMessageChannel channel)
                {
                    _ = Task.Run(async () => await channel.SendMessageAsync(new LocalMessage()
                    {
                        Content = rewardText,
                        AllowedMentions = new(new()
                        {
                            UserIds = new([member.Id]),
                        })
                    }));
                }
            }
        }

        public int GetPlayPower(IMember member)
        {
            ILiteCollection<AnomalyRecord> anomalyRecords = Database.GetCollection<AnomalyRecord>("anomaly");
            AnomalyRecord memberRecord = FindOrCreateRecord(anomalyRecords, member);
            return memberRecord.PlayPower;
        }

        public Task SetPlayPower(IMember member, int pp)
        {
            return Task.Run(() =>
            {
                ILiteCollection<AnomalyRecord> anomalyRecords = Database.GetCollection<AnomalyRecord>("anomaly");
                AnomalyRecord memberRecord = FindOrCreateRecord(anomalyRecords, member);
                memberRecord.PlayPower = Math.Clamp(pp, 0, 6);
                anomalyRecords.Update(memberRecord);
            });
        }

        public Task AddPlayPower(IMember member, int pp)
        {
            return Task.Run(() =>
            {
                ILiteCollection<AnomalyRecord> anomalyRecords = Database.GetCollection<AnomalyRecord>("anomaly");
                AnomalyRecord memberRecord = FindOrCreateRecord(anomalyRecords, member);

                int addedPlayPower = Math.Clamp(pp, 0, 6);
                memberRecord.PlayPower += addedPlayPower;

                ILiteCollection<AnomalyStatisticsRecord> anomalyStatisticsRecords = Database.GetCollection<AnomalyStatisticsRecord>("anomalyStats");
                AnomalyStatisticsRecord memberStatisticsRecord = FindOrCreateStatisticsRecord(anomalyStatisticsRecords, member);
                memberStatisticsRecord.PlayPowerGenerated += addedPlayPower;

                anomalyRecords.Update(memberRecord);
            });
        }

        public Task SubtractPlayPower(IMember member, int pp)
        {
            return Task.Run(() =>
            {
                ILiteCollection<AnomalyRecord> anomalyRecords = Database.GetCollection<AnomalyRecord>("anomaly");
                AnomalyRecord memberRecord = FindOrCreateRecord(anomalyRecords, member);

                int subtractedPlayPower = Math.Clamp(pp, 0, 6);
                memberRecord.PlayPower -= subtractedPlayPower;

                ILiteCollection<AnomalyStatisticsRecord> anomalyStatisticsRecords = Database.GetCollection<AnomalyStatisticsRecord>("anomalyStats");
                AnomalyStatisticsRecord memberStatisticsRecord = FindOrCreateStatisticsRecord(anomalyStatisticsRecords, member);
                memberStatisticsRecord.PlayPowerUsed += subtractedPlayPower;

                anomalyRecords.Update(memberRecord);
            });
        }

        public bool HasPlayPower(IMember member, int pp)
        {
            int memberPp = GetPlayPower(member);
            return (memberPp - pp) >= 0;
        }

        public async Task<bool> SpendPlayPower(IMember member, int pp)
        {
            if (HasPlayPower(member, pp))
            {
                await SubtractPlayPower(member, pp);
                return true;
            }
            else
            {
                Console.WriteLine("poor ass mf");
                return false;
            }
        }

        public Task SetGuarded(IMember member, bool guarded)
        {
            ILiteCollection<AnomalyRecord> anomalyRecords = Database.GetCollection<AnomalyRecord>("anomaly");
            AnomalyRecord memberRecord = FindOrCreateRecord(anomalyRecords, member);
            memberRecord.Guarded = guarded;
            anomalyRecords.Update(memberRecord);
            return Task.CompletedTask;
        }

        public bool IsGuarded(IMember member)
        {
            ILiteCollection<AnomalyRecord> anomalyRecords = Database.GetCollection<AnomalyRecord>("anomaly");
            AnomalyRecord memberRecord = FindOrCreateRecord(anomalyRecords, member);
            return memberRecord.Guarded;
        }

        public async Task BecomeAnomaly(IMember member, AnomalyType? anomaly)
        {
            Task<IChannel?> channelTask = Bot.GetOrFetchChannel(_options.AnnouncementGuild, _options.AnnouncementChannel);

            AnomalyType anomalyType = anomaly ?? Enum.GetValues<AnomalyType>()[Random.Shared.Next(Enum.GetValues<AnomalyType>().Length)];
            ILiteCollection<AnomalyRecord> anomalyRecords = Database.GetCollection<AnomalyRecord>("anomaly");
            AnomalyRecord memberRecord = FindOrCreateRecord(anomalyRecords, member);
            memberRecord.Anomaly = anomalyType;
            anomalyRecords.Update(memberRecord);

            if (await channelTask is IMessageChannel channel)
            {
                string anomalyName = anomalyType switch
                {
                    AnomalyType.Mute => "Demon God",
                    AnomalyType.Channel => "Awakened",
                    _ => "The Ruler",
                };

                await channel.SendMessageAsync(new LocalMessage()
                {
                    Content = $"⚠️ {member.Mention} has become **\"{anomalyName}\"**.",
                    AllowedMentions = new(new()
                    {
                        UserIds = new([member.Id]),
                    })
                });
            }

        }

        public async Task UpgradeAnomaly(AbstractAnomaly firstAnomaly, AbstractAnomaly secondAnomaly, AbstractAnomaly? thirdAnomaly)
        {
            Task<IChannel?> channelTask = Bot.GetOrFetchChannel(_options.AnnouncementGuild, _options.AnnouncementChannel);
            ILiteCollection<AnomalyRecord> anomalyRecords = Database.GetCollection<AnomalyRecord>("anomaly");

            AnomalyTier upgradeTier;
            if (thirdAnomaly is not null)
            {
                upgradeTier = AnomalyTier.RealityThreatening;
            }
            else
            {
                upgradeTier = AnomalyTier.Joined;
            }
            if (firstAnomaly.Record.Tier == upgradeTier || secondAnomaly.Record.Tier == upgradeTier || (thirdAnomaly != null && thirdAnomaly.Record.Tier == upgradeTier))
            {
                return;
            }

            firstAnomaly.Record.Tier = upgradeTier;
            secondAnomaly.Record.Tier = upgradeTier;

            if (thirdAnomaly is not null)
            {
                thirdAnomaly.Record.Tier = upgradeTier;

                firstAnomaly.Record.Partner = AnomalyType.None;
                secondAnomaly.Record.Partner = AnomalyType.None;
                thirdAnomaly.Record.Partner = AnomalyType.None;
            }
            else
            {
                firstAnomaly.Record.Partner = secondAnomaly.Record.Anomaly;
                secondAnomaly.Record.Partner = firstAnomaly.Record.Anomaly;
            }

            anomalyRecords.Update(firstAnomaly.Record);
            anomalyRecords.Update(secondAnomaly.Record);
            if (thirdAnomaly is not null)
            {
                anomalyRecords.Update(thirdAnomaly.Record);
            }

            if (await channelTask is IMessageChannel channel)
            {
                string content;
                List<Snowflake> allowedMentions;
                if (thirdAnomaly is null)
                {
                    content = $"""
                    ⚠️⚠️ {firstAnomaly.Member.Mention} has become **"{firstAnomaly.Name}"**.
                    ⚠️⚠️ {secondAnomaly.Member.Mention} has become **"{secondAnomaly.Name}"**.
                    """;
                    allowedMentions = [firstAnomaly.Member.Id, secondAnomaly.Member.Id];
                }
                else
                {
                    content = $"""
                    ⚠️⚠️⚠️ {firstAnomaly.Member.Mention} has become **"{firstAnomaly.Name}"**.
                    ⚠️⚠️⚠️ {secondAnomaly.Member.Mention} has become **"{secondAnomaly.Name}"**.
                    ⚠️⚠️⚠️ {thirdAnomaly.Member.Mention} has become **"{thirdAnomaly.Name}"**.
                    """;
                    allowedMentions = [firstAnomaly.Member.Id, secondAnomaly.Member.Id, thirdAnomaly.Member.Id];
                }
                await channel.SendMessageAsync(new LocalMessage()
                {
                    Content = content,
                    AllowedMentions = new(new()
                    {
                        UserIds = new(allowedMentions),
                    })
                });
            }

        }

        public Task AddPaintedRole(IMember member, Snowflake id)
        {
            ILiteCollection<AnomalyRecord> anomalyRecords = Database.GetCollection<AnomalyRecord>("anomaly");
            AnomalyRecord memberRecord = FindOrCreateRecord(anomalyRecords, member);
            memberRecord.MostRecentPaintedRole = id;
            anomalyRecords.Update(memberRecord);
            return Task.CompletedTask;
        }

        public Task UpdateRecord(AnomalyRecord record)
        {
            return Task.Run(() =>
            {
                ILiteCollection<AnomalyRecord> anomalyRecords = Database.GetCollection<AnomalyRecord>("anomaly");
                anomalyRecords.Update(record);
            });
        }

        public static AnomalyRecord FindOrCreateRecord(ILiteCollection<AnomalyRecord> collection, IMember member)
        {
            if (collection.FindOne(x => x.GuildId == member.GuildId && x.UserId == member.Id) is AnomalyRecord record)
            {
                return record;
            }
            else
            {
                AnomalyRecord newRecord = new()
                {
                    GuildId = member.GuildId,
                    UserId = member.Id,
                    Anomaly = AnomalyType.None,
                    Partner = AnomalyType.None,
                    Tier = AnomalyTier.Base,
                    Zebras = 0,
                    PlayPower = 2,
                };
                newRecord.RecordId = collection.Insert(newRecord);
                return newRecord;
            }
        }

        public static AnomalyStatisticsRecord FindOrCreateStatisticsRecord(ILiteCollection<AnomalyStatisticsRecord> collection, IMember member)
        {
            if (collection.FindOne(x => x.GuildId == member.GuildId && x.UserId == member.Id) is AnomalyStatisticsRecord record)
            {
                return record;
            }
            else
            {
                AnomalyStatisticsRecord newRecord = new()
                {
                    GuildId = member.GuildId,
                    UserId = member.Id,
                };
                newRecord.RecordId = collection.Insert(newRecord);
                return newRecord;
            }
        }
    }
}
