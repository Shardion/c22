namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Vandalize
{
    public class PaintTwoAction : PaintAction
    {
        public override string Name => "Paint β";
        public override string Description => "Creates a new role and assigns up to 5 members to it. (Change name and color with `/anomaly paint`.)";
        public override bool LockToStarter => false;
        public override int MaximumTargets => 5;

        public PaintTwoAction(AbstractAnomaly anomaly, AnomalyTier tier) : base(anomaly, tier)
        {
        }
    }
}
