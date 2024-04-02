namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Channel
{
    public class MoveTwoAction : MoveAction
    {
        public override string Name => "Move β";
        public override string Description => "Moves a channel and syncs permissions.";

        public override bool SyncPermissions => true;

        public MoveTwoAction(AbstractAnomaly anomaly, AnomalyTier tier) : base(anomaly, tier)
        {
        }
    }
}
