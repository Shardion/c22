namespace Shardion.Achromatic.Configuration
{
    public sealed class DatabaseOptions : IBindableOptions
    {
        public string GetSectionName() => "Database";
        public OptionsAccessibility GetAccessibility() => OptionsAccessibility.NoOne;

        public string ConnectionString { get; set; } = Program.ResolveConfigLocation("Achromatic", ".db");
    }
}
