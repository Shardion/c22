using Shardion.Achromatic.Configuration;

namespace Shardion.Achromatic.Features.Anomaly
{
    public sealed class AnomalyOptions : IBindableOptions
    {
        public string GetSectionName() => "Anomaly";
        public OptionsAccessibility GetAccessibility() => OptionsAccessibility.NoOne;
        public string Emoji { get; set; } = "🐴";

        public ulong AnnouncementChannel { get; set; } = 1168743342507556954; // containment zone (should be important shit)
        public ulong AnnouncementGuild { get; set; } = 1005478985070817410;

        public int PlayPowerRestorationIntervalMinutes { get; set; } = 60;
        public int PlayPowerPerInterval { get; set; } = 2;
        public int MaximumPlayPower { get; set; } = 6;

        public int ZebrasPerAction { get; set; } = 5;
        public int RequiredHorsesToCancelAction { get; set; } = 3;

        public bool ImmediatelyAnomalizeUponStart { get; set; } = false;

        public ulong FirstRoleRewardId { get; set; } = 1168735107310440558; // operator (should be zebra)
        public ulong SecondRoleRewardId { get; set; } = 1168746441699758080; // heightened permissions (should be access to screm)
    }
}
