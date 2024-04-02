using System;

namespace Shardion.Achromatic.Features.Votemute
{
    public class VotemuteMessageStatus
    {
        public DateTimeOffset CreationTimestamp { get; set; } = DateTime.UtcNow;
        public int Reactions { get; set; }
    }
}
