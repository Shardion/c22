using System;
using System.Threading.Tasks;
using Disqord;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Mute
{
    public class BolsterAction : AbstractAnomalyAction
    {
        public override string Name => "Bolster";
        public override string Description => "Protects other Anomaly Actions from being cancelled.";

        public override int Cost => TierProperties.GetForTier(Tier).ActionCost;
        public override int Payout => 0;
        public override int ChargeSeconds => 0;
        public override bool ChargeNeeded => false;

        public BolsterAction(AbstractAnomaly anomaly, AnomalyTier tier) : base(anomaly, tier)
        {
        }

        public override Task Complete()
        {
            foreach (Snowflake snowflake in Anomaly.Manager.MessageActionStatus.Keys)
            {
                Anomaly.Manager.MessageActionStatus[snowflake] = new()
                {
                    Status = ActionCompletionStatus.Successful,
                };
            }
            return Task.CompletedTask;
        }

        public override AbstractReturnableView? GetPreparationView(Func<Task>? returnTo)
        {
            return null;
        }
    }
}
