namespace Shardion.Achromatic.Configuration
{
    public enum OptionsAccessibility
    {
        Internal,
        Public
    }

    public interface IBindableOptions
    {
        string GetSectionName();
        OptionsAccessibility GetAccessibility();
    }
}
