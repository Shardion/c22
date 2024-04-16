using System;
using System.Globalization;
using System.Threading.Tasks;
using Disqord.Bot.Commands.Application;
using Flurl.Util;
using LiteDB;
using Qmmands;
using Shardion.Achromatic.Common.Timers;

namespace Shardion.Achromatic.Features.Reminders
{
    public class RemindersModule : DiscordApplicationGuildModuleBase
    {
        [SlashCommand("remind")]
        [Description("Pings you at a specified time in the future, with optional reminder text.")]
        public ValueTask<IResult> Remind(string time, string text)
        {
            // string reminderText = text.GetValueOrDefault(REMINDER_TEXT_PLACEHOLDERS[Random.Shared.Next(REMINDER_TEXT_PLACEHOLDERS.Length)]);

            DateTimeOffset? nullableParsedTime = ParseRelativeTime(time);

            if (nullableParsedTime is not DateTimeOffset parsedTime)
            {
                return ValueTask.FromResult<IResult>(Response("Invalid time."));
            }
            if (parsedTime < DateTimeOffset.UtcNow)
            {
                return ValueTask.FromResult<IResult>(Response("Specified time has already passed."));
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

            return ValueTask.FromResult<IResult>(Response($"Reminder set for **<t:{parsedTime.ToUnixTimeSeconds()}:F>**!\n> {text}"));
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
            return DateTimeOffset.UtcNow + parsedTime;
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
