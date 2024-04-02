using Disqord;
using LiteDB;

namespace Shardion.Achromatic.Features.Anomaly
{
    public sealed class AnomalyStatisticsRecord
    {
        [BsonId]
        public int RecordId { get; set; }

        public Snowflake UserId { get; set; }
        public Snowflake GuildId { get; set; }

        public int ActionsUsed { get; set; }
        public int ActionsUsedSuccessful { get; set; }
        public int ActionsUsedCancelled { get; set; }
        public int ActionsStopped { get; set; }
        public int ZebrasEarned { get; set; }
        public int PlayPowerGenerated { get; set; }
        public int PlayPowerUsed { get; set; }
        public int PlayPowerWasted { get; set; }
        public int ChannelsCreated { get; set; }
        public int ChannelsMoved { get; set; }
        public int ChannelsReshaped { get; set; }
        public int PermissionsSynced { get; set; }
        public int TimesProtected { get; set; }
        public int TimesProtectedOthers { get; set; }
        public int TimesMuted { get; set; }
        public int TimesMutedOthers { get; set; }
        public int TimesFused { get; set; }
    }
}
