using Disqord;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Channel
{
    public enum OverwriteIntention
    {
        Positive,
        Negative,
    }

    public class OverwriteContainer
    {
        public required LocalOverwrite Overwrite { get; set; }
        public required OverwriteIntention Intention { get; set; }
    }
}
