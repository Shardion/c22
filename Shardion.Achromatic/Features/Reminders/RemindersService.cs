using System;
using System.Threading.Tasks;
using LiteDB;
using Disqord;
using Disqord.Rest;
using Disqord.Bot.Hosting;
using Shardion.Achromatic.Common.Timers;
using Shardion.Achromatic.Extensions;
using Shardion.Achromatic.Configuration;
using Microsoft.Extensions.Logging;

namespace Shardion.Achromatic.Features.Reminders
{
    public class RemindersService : DiscordBotService
    {
        public RemindersService(OptionsMultiplexer opt)
        {
            AchromaticTimer.TimerExpired += async (t) =>
            {
                try
                {

                    if (t.Identifier != "reminder")
                    {
                        return;
                    }

                    Snowflake? uid;
                    if (!t.Document.TryGetValue("uid", out BsonValue unparsedUid) || !unparsedUid.IsString || !Snowflake.TryParse(unparsedUid.AsString, out Snowflake parsedUid))
                    {
                        uid = null;
                    }
                    else
                    {
                        uid = parsedUid;
                    }

                    Task<IUser?> targetUserTask;
                    if (uid is Snowflake validUid)
                    {
                        targetUserTask = Bot.GetOrFetchUser(validUid);
                    }
                    else
                    {
                        targetUserTask = Task.FromResult<IUser?>(null);
                    }

                    Snowflake? cid;
                    if (!t.Document.TryGetValue("cid", out BsonValue unparsedCid) || !unparsedCid.IsString || !Snowflake.TryParse(unparsedCid.AsString, out Snowflake parsedCid))
                    {
                        cid = null;
                    }
                    else
                    {
                        cid = parsedCid;
                    }

                    Snowflake? gid;
                    if (!t.Document.TryGetValue("gid", out BsonValue unparsedGid) || !unparsedGid.IsString || !Snowflake.TryParse(unparsedGid.AsString, out Snowflake parsedGid))
                    {
                        gid = null;
                    }
                    else
                    {
                        gid = parsedGid;
                    }

                    LocalMessage preparedMessage = new();

                    Task<IChannel?> targetChannelTask;
                    if (cid is Snowflake validCid && gid is Snowflake validGid)
                    {
                        targetChannelTask = Bot.GetOrFetchChannel(validGid, validCid);
                    }
                    else
                    {
                        targetChannelTask = Task.FromResult<IChannel?>(null);
                    }

                    DateTimeOffset? startTime;
                    if (!t.Document.TryGetValue("startTime", out BsonValue unparsedStartTime) || !unparsedStartTime.IsDateTime)
                    {
                        startTime = null;
                    }
                    else
                    {
                        startTime = new DateTimeOffset(unparsedStartTime.AsDateTime);
                    }

                    string? reminderText;
                    if (!t.Document.TryGetValue("text", out BsonValue text) || !text.IsString)
                    {
                        reminderText = null;
                    }
                    else
                    {
                        reminderText = text.AsString;
                    }

                    string reminderTimeLine;
                    if (startTime is not DateTimeOffset validStartTime)
                    {
                        reminderTimeLine = "**Reminder from unknown time!**";
                    }
                    else
                    {
                        reminderTimeLine = $"**Reminder from <t:{validStartTime.ToUnixTimeSeconds()}:F>**!";
                    }

                    string reminderTextLine;
                    if (reminderText is null)
                    {
                        reminderTextLine = "I failed to load the reminder text for this reminder. Oops. You might be able to recover it by finding the original message.";
                    }
                    else
                    {
                        reminderTextLine = $"> {reminderText}";
                    }

                    string reminderMentionLine;
                    IUser? targetUser = await targetUserTask;
                    if (targetUser is null)
                    {
                        reminderMentionLine = "*Unknown user. If you know who this reminder was for, ping them!*";
                    }
                    else
                    {
                        reminderMentionLine = targetUser.Mention;
                        preparedMessage.AllowedMentions = new(new()
                        {
                            UserIds = new([targetUser.Id]),
                        });
                    }

                    preparedMessage.Content = $"{reminderMentionLine}\n{reminderTimeLine}\n{reminderTextLine}";

                    IChannel? targetChannel = await targetChannelTask;
                    if (targetChannel is IMessageChannel validChannel)
                    {
                        await validChannel.SendMessageAsync(preparedMessage);
                    }
                    else
                    {
                        if (targetUser is not null)
                        {
                            await targetUser.SendMessageAsync(preparedMessage);
                        }
                        else
                        {
                            RemindersOptions remindersOptions = opt.Get<RemindersOptions>(OptionsAccessibility.Internal, null, null) ?? new();
                            if (await Bot.GetOrFetchChannel(remindersOptions.UnknownReminderGuildId, remindersOptions.UnknownReminderChannelId) is IMessageChannel validFallbackChannel)
                            {
                                await validFallbackChannel.SendMessageAsync(preparedMessage);
                            }
                            else
                            {
                                Logger.LogError("Really screwed up and dropped reminder {}!!!", t.TimerId);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e);
                }
            };
        }
    }
}
