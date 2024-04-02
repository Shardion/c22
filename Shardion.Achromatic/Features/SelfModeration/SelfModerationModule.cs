using System.Collections.Concurrent;
using System.Threading.Tasks;
using System;
using Disqord;
using Disqord.Bot.Commands.Application;
using Disqord.Rest;
using Qmmands;
using Shardion.Achromatic.Extensions;

namespace Shardion.Achromatic.Features.SelfModeration
{
    public class SelfModerationModule : DiscordApplicationGuildModuleBase
    {
        [SlashCommand("selfpurge")]
        [Description("Purges your messages sent in a specified timeframe.")]
        public async ValueTask<IResult> SelfPurge(int minutes)
        {
            await Context.Interaction.Response().SendMessageAsync(new() { Content = "Purging...", IsEphemeral = true });
            DateTimeOffset cutoffDate = DateTimeOffset.UtcNow.AddMinutes(-minutes);
            if (await Bot.GetOrFetchChannel(Context.GuildId, Context.ChannelId) is not IMessageGuildChannel ch)
            {
                return new Result("Current channel does not support messages. (But how did you run the command??)");
            }

            ConcurrentBag<Snowflake> targetedMessageList = [];
            await Parallel.ForEachAsync(ch.EnumerateMessages(1000), (messageChunk, cancellation) =>
            {
                Parallel.ForEach(messageChunk, (message) =>
                {
                    if (message.CreatedAt() > cutoffDate && message.Author.Id == Context.Author.Id)
                    {
                        targetedMessageList.Add(message.Id);
                    }
                });
                return ValueTask.CompletedTask;
            });
            await ch.DeleteMessagesAsync(targetedMessageList);
            await Context.Interaction.Followup().ModifyResponseAsync((a) =>
            {
                a.Content = $"Purged. Removed {targetedMessageList.Count} messages.";
            });
            return new Result();
        }
    }
}
