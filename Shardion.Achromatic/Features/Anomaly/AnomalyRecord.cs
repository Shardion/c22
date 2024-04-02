using System;
using Disqord;
using LiteDB;
using Shardion.Achromatic.Features.Anomaly.Anomalies;

namespace Shardion.Achromatic.Features.Anomaly
{
    public enum AnomalyType
    {
        None,
        Mute,
        Channel,
        Vandalize,
    }

    public sealed class AnomalyRecord
    {
        [BsonId]
        public int RecordId { get; set; }

        public Snowflake UserId { get; set; }
        public Snowflake GuildId { get; set; }

        public AnomalyType Anomaly { get; set; }
        public AnomalyTier Tier { get; set; }
        public AnomalyType Partner { get; set; }

        public int Zebras { get; set; }
        public int PlayPower { get; set; }

        public Snowflake MostRecentPaintedRole { get; set; }

        public bool Guarded { get; set; }
        public bool EarnedFirstRole { get; set; }
        public bool EarnedSecondRole { get; set; }
    }
}
