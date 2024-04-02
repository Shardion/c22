using Shardion.Achromatic.Configuration;

namespace Shardion.Achromatic.Features.Reminders
{
    public sealed class RemindersOptions : IBindableOptions
    {
        public string GetSectionName() => "Reminders";
        public OptionsAccessibility GetAccessibility() => OptionsAccessibility.NoOne;

        public ulong UnknownReminderChannelId = 712427632632791050;
        public ulong UnknownReminderGuildId = 712393733995495464;
    }
}
