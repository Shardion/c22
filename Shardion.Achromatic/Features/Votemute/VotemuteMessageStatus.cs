using System;
using System.Collections.Generic;
using Disqord;

namespace Shardion.Achromatic.Features.Votemute
{
    public class VotemuteMessageStatus
    {
        public DateTimeOffset CreationTimestamp { get; set; } = DateTime.UtcNow;
        public ISet<Snowflake> Reactors { get; set; } = new HashSet<Snowflake>();
        public bool Old => CreationTimestamp.AddMinutes(10) < DateTime.UtcNow;
    }
}
