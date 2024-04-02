namespace Shardion.Achromatic.Features.Anomaly.Anomalies
{
    public readonly record struct TierProperties(string Name, int ActionCost, int ActionSeconds, int ActionPayout, int Color)
    {
        public static TierProperties GetForTier(AnomalyTier tier) => tier switch
        {
            AnomalyTier.Base => new("Base", 1, 20, 5, 0x00ff00),
            AnomalyTier.Joined => new("Joined", 2, 20, 5, 0xffff00),
            _ => new("Reality-Threatening", 3, 20, 5, 0xff0000),
        };
    }
}
