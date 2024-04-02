using System;
using System.Threading.Tasks;
using Disqord;
using LiteDB;
using Shardion.Achromatic.Extensions;
using Shardion.Achromatic.Features.Anomaly.Shop;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Mute
{
    public class GuardAction : AbstractAnomalyAction
    {
        public override string Name => "Guard";
        public override string Description => "Protects a member from being punished or disadvantaged.";

        public override int Cost => TierProperties.GetForTier(Tier).ActionCost;
        public override int Payout => TierProperties.GetForTier(Tier).ActionPayout;
        public override int ChargeSeconds => TierProperties.GetForTier(Tier).ActionSeconds;
        public override bool ChargeNeeded => true;

        public ISnowflakeEntity? GuardTarget { get; set; }

        public GuardAction(AbstractAnomaly anomaly, AnomalyTier tier) : base(anomaly, tier)
        {
        }

        public override async Task Complete()
        {
            if (GuardTarget is null)
            {
                return;
            }
            IMember? guardTargetMember = await Anomaly.Bot.GetOrFetchMember(Anomaly.Member.GuildId, GuardTarget.Id);
            if (guardTargetMember is not null)
            {
                await Anomaly.Database.SetGuarded(guardTargetMember, true);
                ILiteCollection<AnomalyStatisticsRecord> records = Anomaly.Database.Database.GetCollection<AnomalyStatisticsRecord>("anomalyStats");
                AnomalyStatisticsRecord record = AnomalyDatabaseService.FindOrCreateStatisticsRecord(records, Anomaly.Member);
                record.TimesProtectedOthers++;
                records.Update(record);
            }
            return;
        }

        public override AbstractReturnableView? GetPreparationView(Func<Task>? returnTo)
        {
            return new GuardActionPreparationView(returnTo, this);
        }
    }
}
