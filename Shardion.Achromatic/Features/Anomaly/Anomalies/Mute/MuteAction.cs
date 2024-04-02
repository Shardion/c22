using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;
using LiteDB;
using Shardion.Achromatic.Extensions;
using Shardion.Achromatic.Features.Anomaly.Shop;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Mute
{
    public class MuteAction : AbstractAnomalyAction
    {
        public override string Name => "Mute ɑ";
        public override string Description => "Mutes a member for 15 minutes.";

        public override int Cost => TierProperties.GetForTier(Tier).ActionCost;
        public override int Payout => TierProperties.GetForTier(Tier).ActionPayout;
        public override int ChargeSeconds => TierProperties.GetForTier(Tier).ActionSeconds;
        public override bool ChargeNeeded => true;
        public virtual int MuteMinutes => 15;
        public virtual int MaxMuteTargets => 1;

        public IReadOnlyCollection<ISnowflakeEntity>? MuteTargets { get; set; }

        public MuteAction(AbstractAnomaly anomaly, AnomalyTier tier) : base(anomaly, tier)
        {
        }

        public override async Task Complete()
        {
            if (MuteTargets is null)
            {
                throw new InvalidOperationException("no mute targets set");
            }
            if (await Anomaly.Bot.GetOrFetchGuild(Anomaly.Context.GuildId) is not IGuild guild)
            {
                throw new InvalidOperationException("failed to get guild coming from a guild-contexted command, wtf");
            }

            List<Task<IMember>> tasks = [];
            foreach (ISnowflakeEntity ent in MuteTargets)
            {
                ILiteCollection<AnomalyStatisticsRecord> records = Anomaly.Database.Database.GetCollection<AnomalyStatisticsRecord>("anomalyStats");
                
                if (await Anomaly.Bot.GetOrFetchMember(Anomaly.Context.GuildId, ent.Id) is IMember member)
                {
                    AnomalyStatisticsRecord targetRecord = AnomalyDatabaseService.FindOrCreateStatisticsRecord(records, member);
                    if (Anomaly.Database.IsGuarded(member))
                    {
                        await Anomaly.Database.SetGuarded(member, false);
                        _ = Task.Run(async () => await member.SendMessageAsync(new()
                        {
                            Content = $"You were guarded from being muted by **{Anomaly.Context.Author.GlobalName ?? Anomaly.Context.Author.Name}**!",
                        }));
                        
                        targetRecord.TimesProtected++;
                        records.Update(targetRecord);
                        continue;
                    }
                    else
                    {
                        targetRecord.TimesMuted++;
                        records.Update(targetRecord);
                    }
                }

                AnomalyStatisticsRecord actionerRecord = AnomalyDatabaseService.FindOrCreateStatisticsRecord(records, Anomaly.Member);
                actionerRecord.TimesMutedOthers++;
                records.Update(actionerRecord);

                tasks.Add(guild.ModifyMemberAsync(ent.Id, (x) =>
                {
                    x.TimedOutUntil = new(DateTimeOffset.UtcNow.AddMinutes(MuteMinutes));
                }));
            }
            await Task.WhenAll(tasks);
        }

        public override AbstractReturnableView? GetPreparationView(Func<Task>? returnTo)
        {
            return new MuteActionPreparationView(returnTo, this);
        }
    }
}
