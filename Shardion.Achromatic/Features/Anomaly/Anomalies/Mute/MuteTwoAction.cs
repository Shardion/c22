namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Mute
{
    public class MuteTwoAction : MuteAction
    {
        public MuteTwoAction(AbstractAnomaly anomaly, AnomalyTier tier) : base(anomaly, tier)
        {
        }

        public override string Name => "Mute β";
        public override string Description => "Mutes up to three members for 15 minutes.";
        public override int MuteMinutes => 15;
        public override int MaxMuteTargets => 3;
    }
}
