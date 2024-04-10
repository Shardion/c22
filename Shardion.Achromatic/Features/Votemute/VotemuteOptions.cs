using Shardion.Achromatic.Configuration;

namespace Shardion.Achromatic.Features.Votemute
{
    public sealed class VotemuteOptions : IBindableOptions
    {
        public string GetSectionName() => "Votemute";
        public OptionsAccessibility GetAccessibility() => OptionsAccessibility.Internal;

        public bool Enabled { get; set; } = true;
        public string Emoji { get; set; } = "🐴";
        public int NumReactions { get; set; } = 3;
        public int MinutesMuted { get; set; } = 5;
    }
}
