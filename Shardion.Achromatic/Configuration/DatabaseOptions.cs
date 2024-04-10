namespace Shardion.Achromatic.Configuration
{
    public sealed class DatabaseOptions : IBindableOptions
    {
        public string GetSectionName() => "Database";
        public OptionsAccessibility GetAccessibility() => OptionsAccessibility.Internal;

        public string ConnectionString { get; set; } = Program.ResolveConfigLocation("Achromatic", ".db");
    }
}
