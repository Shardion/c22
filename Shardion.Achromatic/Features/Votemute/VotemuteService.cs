using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Disqord.Rest;
using Shardion.Achromatic.Extensions;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Shardion.Achromatic.Configuration;

namespace Shardion.Achromatic.Features.Votemute
{
    public enum MuteReason
    {
        ReachedReactionThreshold,
        DeletedMessageWithReaction,
    }

    public class VotemuteService : DiscordBotService
    {
        public ConcurrentDictionary<Snowflake, VotemuteMessageStatus> MessageHorseCounter = [];
        private readonly Mutex _gcMutex = new();
        private readonly OptionsMultiplexer _options;

        public VotemuteService(OptionsMultiplexer options)
        {
            _options = options;
        }

        protected override async ValueTask OnReactionAdded(ReactionAddedEventArgs e)
        {
            if (e.MessageId is Snowflake messageId && e.ChannelId is Snowflake channelId && e.Member is IMember member)
            {
                VotemuteOptions? options = _options.Get<VotemuteOptions>(OptionsAccessibility.Everyone, null, e.GuildId);
                if (options is null || !options.Enabled)
                {
                    return;
                }

                if (e.Emoji.Name == options.Emoji)
                {
                    IMessage? message = await Bot.GetOrFetchMessage(channelId, messageId);
                    if (message is null)
                    {
                        return;
                    }

                    if (message.Author.Id == e.UserId)
                    {
                        return;
                    }

                    await AddMuteReaction(member, message);

                    if (_gcMutex.WaitOne(250))
                    {
                        // Reactions are very rare in Ench, so waiting on this is *probably* fine.
                        // This method should not take very long to complete.
                        await GarbageCollectStatuses();
                        _gcMutex.ReleaseMutex();
                    }
                }
            }
        }

        protected override async ValueTask OnMessageDeleted(MessageDeletedEventArgs e)
        {
            if (e.Message is not CachedUserMessage message || e.GuildId is not Snowflake guildId)
            {
                return;
            }
            if (await Bot.GetOrFetchMember(guildId, message.Author.Id) is not IMember member)
            {
                return;
            }

            VotemuteOptions? options = _options.Get<VotemuteOptions>(OptionsAccessibility.Everyone, null, e.GuildId);
            if (options is null || !options.Enabled)
            {
                return;
            }


            if (MessageHorseCounter.TryGetValue(e.MessageId, out VotemuteMessageStatus? nullableStatus) && nullableStatus is VotemuteMessageStatus status)
            {
                if (status.Reactions > (options.NumReactions / 2.0) && status.Reactions < options.NumReactions)
                {
                    await Mute(member, MuteReason.DeletedMessageWithReaction);
                }
            }
        }

        private bool IsStatusTooOld(Snowflake messageId)
        {
            if (MessageHorseCounter[messageId].CreationTimestamp.AddMinutes(10) < DateTime.UtcNow)
            {
                return true;
            }
            return false;
        }

        private async Task GarbageCollectStatuses(CancellationToken cancellationToken = default)
        {
            ConcurrentBag<Snowflake> collectableStatusKeys = [];
            IEnumerable<KeyValuePair<Snowflake, VotemuteMessageStatus>> collectableStatuses = MessageHorseCounter.Where((status) => IsStatusTooOld(status.Key));
            await Parallel.ForEachAsync(collectableStatuses, (pair, token) =>
            {
                collectableStatusKeys.Add(pair.Key);
                return ValueTask.CompletedTask;
            });
            await Parallel.ForEachAsync(collectableStatusKeys, cancellationToken, (id, token) =>
            {
                MessageHorseCounter.Remove(id, out _);
                return ValueTask.CompletedTask;
            });
        }

        private async ValueTask AddMuteReaction(IMember member, IMessage message, int horses = 1, CancellationToken cancellationToken = default)
        {
            VotemuteMessageStatus newStatus = new()
            {
                CreationTimestamp = message.CreatedAt()
            };
            MessageHorseCounter[message.Id].Reactions = MessageHorseCounter.GetOrAdd(message.Id, newStatus).Reactions + horses;

            VotemuteOptions? options = _options.Get<VotemuteOptions>(OptionsAccessibility.Everyone, null, member.GuildId);
            if (options is null)
            {
                return;
            }

            if (!IsStatusTooOld(message.Id) && MessageHorseCounter[message.Id].Reactions >= options.NumReactions)
            {
                await Mute(member, MuteReason.ReachedReactionThreshold, cancellationToken);
            }
        }

        private async ValueTask Mute(IMember member, MuteReason reason, CancellationToken cancellationToken = default)
        {
            VotemuteOptions? options = _options.Get<VotemuteOptions>(OptionsAccessibility.Everyone, null, member.GuildId);
            if (options is null)
            {
                return;
            }

            IRestRequestOptions opt = new DefaultRestRequestOptions()
                .WithReason(reason switch
                {
                    MuteReason.ReachedReactionThreshold => $"Had {options.NumReactions} votemute reactions on message",
                    MuteReason.DeletedMessageWithReaction => "Deleted message with votemute reactions",
                    _ => "Muted by votemute",
                });

            await member.ModifyAsync((a) =>
            {
                a.TimedOutUntil = new Qommon.Optional<DateTimeOffset?>(DateTimeOffset.Now.AddMinutes(options.MinutesMuted));
            }, opt, cancellationToken: cancellationToken);
        }

        ~VotemuteService()
        {
            _gcMutex.Dispose();
        }
    }
}
