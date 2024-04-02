using System;
using System.Globalization;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands.Application;
using Disqord.Rest;
using Flurl.Util;
using LiteDB;
using Qmmands;
using Qommon;
using Shardion.Achromatic.Common.Timers;

namespace Shardion.Achromatic.Features.Reminders
{
    public class RemindersModule : DiscordApplicationGuildModuleBase
    {
        private static readonly string[] REMINDER_TEXT_PLACEHOLDERS =
        [
            "Scary rumor: I'm working on a song named GHOUL",
            "I've come to make an announcement.",
            "[object Object]",
            "undefined",
            "In the boundless expanse of the multiverse,",
            "<:22:773603249633755186>",
            "https://cdn.discordapp.com/attachments/712427632632791050/1221865793889898667/caption.gif?ex=66142232&is=6601ad32&hm=fec8fd2101ad3c318c6e5f93a81091494129931b772098953d83859fe6ec8269&",
            "https://cdn.discordapp.com/attachments/712427632632791050/1223311940017983701/caption.gif?ex=66196506&is=6606f006&hm=4a98fc5c3cc13a4192aba52ed8a7dffb5e73cc1422d1c680baab99bae84c013e&",
        ];

        [SlashCommand("remind")]
        [Description("Pings you at a specified time in the future, with optional reminder text.")]
        public ValueTask<IResult> Remind(string time, string text)
        {
            // string reminderText = text.GetValueOrDefault(REMINDER_TEXT_PLACEHOLDERS[Random.Shared.Next(REMINDER_TEXT_PLACEHOLDERS.Length)]);

            if (ParseRelativeTime(time) is not DateTimeOffset parsedTime)
            {
                return ValueTask.FromResult<IResult>(Response("Invalid time. ^(Tip: Specifying with months and beyond are not supported! Use a number of days or weeks!)"));
            }

            BsonDocument timerInfo = new()
            {
                ["text"] = text,
                ["startTime"] = DateTimeOffset.UtcNow.UtcDateTime,
                ["uid"] = Context.AuthorId.RawValue.ToInvariantString(),
                ["cid"] = Context.ChannelId.RawValue.ToInvariantString(),
                ["gid"] = Context.GuildId.RawValue.ToInvariantString(),
            };
            AchromaticTimer timer = new()
            {
                Identifier = "reminder",
                CompletionTime = parsedTime,
                Document = timerInfo,
            };
            timer.Start();

            return ValueTask.FromResult<IResult>(Response($"Reminder set for **<t:{parsedTime.ToUnixTimeSeconds()}:F>**!"));
        }

        private static DateTimeOffset? ParseRelativeTime(string reminderTime)
        {
            string[] reminderTimeComponents = reminderTime.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            TimeSpan parsedTime = TimeSpan.Zero;
            foreach (string timeComponent in reminderTimeComponents)
            {
                if (ParseHours(timeComponent) is TimeSpan hoursSpan)
                {
                    parsedTime += hoursSpan;
                    continue;
                }
                if (ParseDays(timeComponent) is TimeSpan daysSpan)
                {
                    parsedTime += daysSpan;
                    continue;
                }
                if (ParseMinutes(timeComponent) is TimeSpan minutesSpan)
                {
                    parsedTime += minutesSpan;
                    continue;
                }
                if (ParseWeeks(timeComponent) is TimeSpan weeksSpan)
                {
                    parsedTime += weeksSpan;
                    continue;
                }
                if (ParseSeconds(timeComponent) is TimeSpan secondsSpan)
                {
                    parsedTime += secondsSpan;
                    continue;
                }
            }
            if (parsedTime == TimeSpan.Zero)
            {
                return null;
            }
            else
            {
                return DateTimeOffset.UtcNow + parsedTime;
            }
        }

        private static TimeSpan? ParseSeconds(string part)
        {
            if (part.EndsWith('s'))
            {
                if (int.TryParse(part[..^1], NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out int parsedNum))
                {
                    return TimeSpan.FromSeconds(parsedNum);
                }
            }
            return null;
        }

        private static TimeSpan? ParseMinutes(string part)
        {
            if (part.EndsWith('m'))
            {
                if (int.TryParse(part[..^1], NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out int parsedNum))
                {
                    return TimeSpan.FromMinutes(parsedNum);
                }
            }
            return null;
        }

        private static TimeSpan? ParseHours(string part)
        {
            if (part.EndsWith('h'))
            {
                if (int.TryParse(part[..^1], NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out int parsedNum))
                {
                    return TimeSpan.FromHours(parsedNum);
                }
            }
            return null;
        }

        private static TimeSpan? ParseDays(string part)
        {
            if (part.EndsWith('d'))
            {
                if (int.TryParse(part[..^1], NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out int parsedNum))
                {
                    return TimeSpan.FromDays(parsedNum);
                }
            }
            return null;
        }

        private static TimeSpan? ParseWeeks(string part)
        {
            if (part.EndsWith('w'))
            {
                if (int.TryParse(part[..^1], NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out int parsedNum))
                {
                    return TimeSpan.FromDays(parsedNum * 7);
                }
            }
            return null;
        }
    }
}
