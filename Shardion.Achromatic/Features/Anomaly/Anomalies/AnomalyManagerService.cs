using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Disqord;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Hosting;
using LiteDB;
using Shardion.Achromatic.Configuration;
using Shardion.Achromatic.Extensions;
using Shardion.Achromatic.Features.Anomaly.Anomalies.Channel;
using Shardion.Achromatic.Features.Anomaly.Anomalies.Mute;
using Shardion.Achromatic.Features.Anomaly.Anomalies.Vandalize;
using Shardion.Achromatic.Features.Anomaly.Shop;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies
{
    public enum AnomalyTier
    {
        Base,
        Joined,
        RealityThreatening,
    }

    public enum ActionCompletionStatus
    {
        Unknown,
        Successful,
        Failed,
    }

    public class ActionStatus
    {
        public ISet<IMember> Users { get; set; } = new HashSet<IMember>();
        public ActionCompletionStatus Status { get; set; } = ActionCompletionStatus.Unknown;
    }

    public class AnomalyManagerService : DiscordBotService
    {
        private readonly Random _rand = new();
        private readonly OptionsMultiplexer _opt;
        private readonly AnomalyOptions _anomalyOpt;
        private readonly AnomalyDatabaseService _db;
        private readonly Timer _ppTimer;
        private readonly Timer _anomalyTimer;
        private bool _firstRun = true;

        private static readonly Snowflake[] ANOMALY_CANDIDATES =
        [
            208129127494975488, // me
            266420011017568266, // plant
            312457232543383552, // eozvar
            335512019337740289, // brim
            365057522446368770, // robber
            432071612116893696, // bbs
            472037524185808906, // derp
            642006237642358814, // shadow puppet
            869160948647206913, // geckronome
            443227282488819733, // tech
            751222490910425169, // arti
        ];

        private static readonly DateTimeOffset DEATH_DATE = new(2024, 4, 1, 0, 0, 0, 0, TimeSpan.Zero);

        public ConcurrentDictionary<Snowflake, ActionStatus> MessageActionStatus = new();
        public ConcurrentDictionary<Snowflake, IMessage> MessageSelections = new();

        public AnomalyManagerService(OptionsMultiplexer opt, AnomalyDatabaseService db)
        {
            _opt = opt;
            _anomalyOpt = _opt.Get<AnomalyOptions>(OptionsAccessibility.NoOne, null, null) ?? new();
            _db = db;
            _ppTimer = new()
            {
                AutoReset = false,
                Enabled = false,
                Interval = 1,
            };
            _ppTimer.Elapsed += (maybeTimer, args) => _ = Task.Run(() =>
            {
                if (maybeTimer is not Timer timer)
                {
                    return;
                }
                if (DEATH_DATE > DateTimeOffset.UtcNow)
                {
                    timer.Interval = (DEATH_DATE - DateTimeOffset.UtcNow).TotalMilliseconds;
                    timer.Start();
                }
                else
                {
                    ILiteCollection<AnomalyRecord> anomalyRecords = _db.Database.GetCollection<AnomalyRecord>("anomaly");
                    foreach (AnomalyRecord record in anomalyRecords.Find((record) => record.PlayPower < 6))
                    {
                        record.PlayPower = Math.Clamp(record.PlayPower += 2, 0, 6);
                        anomalyRecords.Update(record);
                    }

                    timer.Interval = TimeSpan.FromMinutes(60).TotalMilliseconds;
                    timer.Start();
                }
            });
            _ppTimer.Start();
            _anomalyTimer = new()
            {
                AutoReset = false,
                Enabled = false,
                Interval = 1,
            };
            _anomalyTimer.Elapsed += async (maybeTimer, args) =>
            {
                if (maybeTimer is not Timer timer)
                {
                    return;
                }
                if (!_anomalyOpt.ImmediatelyAnomalizeUponStart && _firstRun)
                {
                    _firstRun = false;
                    timer.Interval = TimeSpan.FromMinutes(60).TotalMilliseconds;
                    timer.Start();
                    return;
                }
                if (DEATH_DATE > DateTimeOffset.UtcNow)
                {
                    timer.Interval = (DEATH_DATE - DateTimeOffset.UtcNow).TotalMilliseconds;
                    timer.Start();
                }
                else
                {
                    ILiteCollection<AnomalyRecord> anomalyRecords = _db.Database.GetCollection<AnomalyRecord>("anomaly");
                    List<AnomalyRecord> candidates = [];
                    foreach (AnomalyRecord record in anomalyRecords.FindAll())
                    {
                        if (ANOMALY_CANDIDATES.Contains(record.UserId) && record.Anomaly == AnomalyType.None)
                        {
                            candidates.Add(record);
                        }
                    }

                    if (candidates.Count > 0)
                    {
                        AnomalyRecord selectedCandidate = candidates[Random.Shared.Next(candidates.Count)];
                        if (await Bot.GetOrFetchMember(selectedCandidate.GuildId, selectedCandidate.UserId) is IMember member)
                        {
                            await _db.BecomeAnomaly(member, null);
                        }
                    }
                    timer.Interval = TimeSpan.FromMinutes(60).TotalMilliseconds;
                    timer.Start();
                }
            };
            _anomalyTimer.Start();
        }

        public AbstractAnomaly? GetAnomaly(IDiscordApplicationGuildCommandContext context, IMember member, AnomalyRecord record, AnomalyType? type = null)
        {
            AnomalyType targetType;
            if (type is not AnomalyType validType)
            {
                targetType = record.Anomaly;
            }
            else
            {
                targetType = validType;
            }

            return targetType switch
            {
                AnomalyType.Mute => new MuteAnomaly(_opt, _db, this, Bot, context, member, record),
                AnomalyType.Channel => new ChannelAnomaly(_opt, _db, this, Bot, context, member, record),
                AnomalyType.Vandalize => new VandalizeAnomaly(_opt, _db, this, Bot, context, member, record),
                _ => null,
            };
        }

        public AbstractAnomaly GetRandomAnomaly(IDiscordApplicationGuildCommandContext context, IMember member, AnomalyRecord record)
        {
            AnomalyType[] types = Enum.GetValues<AnomalyType>();
            AnomalyType targetType = types[_rand.Next(types.Length)];
            return targetType switch
            {
                AnomalyType.Mute => new MuteAnomaly(_opt, _db, this, Bot, context, member, record),
                AnomalyType.Channel => new ChannelAnomaly(_opt, _db, this, Bot, context, member, record),
                _ => new VandalizeAnomaly(_opt, _db, this, Bot, context, member, record),
            };
        }
    }
}
