namespace Shardion.Achromatic.Configuration
{
    public sealed class IdentityOptions : IBindableOptions
    {
        public string GetSectionName() => "Identity";
        public OptionsAccessibility GetAccessibility() => OptionsAccessibility.Internal;

        public ulong? PrimaryDeveloperID { get; set; } = null;
        public ulong? PrimaryDevelopmentServerID { get; set; } = null;
        public ulong[] DeveloperIDs { get; set; } = [];
        public ulong[] DevelopmentServerIDs { get; set; } = [];

        public string Name { get; set; } = "C22";
        public int Color { get; set; } = 0x1f1e33;
    }
}
