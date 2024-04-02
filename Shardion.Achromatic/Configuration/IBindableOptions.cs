namespace Shardion.Achromatic.Configuration
{
    public enum OptionsAccessibility
    {
        NoOne, // internal stuff like tokens, nobody r/w
        Servers, // servers r/w
        Users, // users r/w
        Everyone // both servers and users r/w
    }

    public interface IBindableOptions
    {
        string GetSectionName();
        OptionsAccessibility GetAccessibility();
    }
}
