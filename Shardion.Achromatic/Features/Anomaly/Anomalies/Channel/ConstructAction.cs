using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;
using LiteDB;
using Shardion.Achromatic.Extensions;
using Shardion.Achromatic.Features.Anomaly.Shop;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Channel
{
    public class ConstructAction : AbstractAnomalyAction
    {
        public override string Name => "Construct ɑ";
        public override string Description => "Constructs a new channel with custom permissions.";

        public override int Cost => TierProperties.GetForTier(Tier).ActionCost;
        public override int Payout => TierProperties.GetForTier(Tier).ActionPayout;
        public override int ChargeSeconds => TierProperties.GetForTier(Tier).ActionSeconds;
        public override bool ChargeNeeded => true;

        public string? ChannelName { get; set; }
        public IReadOnlyList<OverwriteContainer>? ChannelPermissionOverwrites { get; set; }

        public ConstructAction(AbstractAnomaly anomaly, AnomalyTier tier) : base(anomaly, tier)
        {
        }

        public override async Task Complete()
        {
            if (ChannelName is null || ChannelPermissionOverwrites is null)
            {
                throw new InvalidOperationException("no channel name or overwrites set");
            }
            if (await Anomaly.Bot.GetOrFetchGuild(Anomaly.Context.GuildId) is not IGuild guild)
            {
                throw new InvalidOperationException("failed to get guild coming from a guild-contexted command, wtf");
            }

            List<LocalOverwrite> overwrites = [];

            foreach (OverwriteContainer cont in ChannelPermissionOverwrites)
            {
                if (cont.Intention == OverwriteIntention.Negative && cont.Overwrite.TargetId.HasValue)
                {
                    if (await Anomaly.Bot.GetOrFetchMember(Anomaly.Context.GuildId, cont.Overwrite.TargetId.Value) is IMember member)
                    {
                        if (Anomaly.Database.IsGuarded(member))
                        {
                            await Anomaly.Database.SetGuarded(member, false);
                            _ = Task.Run(async () => await member.SendMessageAsync(new()
                            {
                                Content = $"You were guarded from being banned from `#{ChannelName}`!",
                            }));
                            ILiteCollection<AnomalyStatisticsRecord> records = Anomaly.Database.Database.GetCollection<AnomalyStatisticsRecord>("anomalyStats");
                            AnomalyStatisticsRecord record = AnomalyDatabaseService.FindOrCreateStatisticsRecord(records, Anomaly.Member);
                            record.TimesProtected++;
                            records.Update(record);
                            continue;
                        }
                    }
                }
                overwrites.Add(cont.Overwrite);
            }

            await guild.CreateTextChannelAsync(ChannelName, (prop) =>
            {
                prop.Overwrites = new(overwrites);
            });
        }

        public override AbstractReturnableView? GetPreparationView(Func<Task>? returnTo)
        {
            return new ConstructActionPreparationView(returnTo, this);
        }
    }
}
