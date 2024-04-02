namespace Shardion.Achromatic.Features.About
{
    public interface IStatsEntry
    {
        string Name { get; }

        string GetStatistic();
    }
}
