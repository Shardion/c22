namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Vandalize
{
    public class DestroyTwoAction : DestroyAction
    {
        public override string Name => "Destroy β";
        public override string Description => "Deletes any message in any channel.";
        public override bool CanDeleteOldMessages => true;
        public override bool LockToCurrentChannel => false;

        public DestroyTwoAction(AbstractAnomaly anomaly, AnomalyTier tier) : base(anomaly, tier)
        {
        }
    }
}
