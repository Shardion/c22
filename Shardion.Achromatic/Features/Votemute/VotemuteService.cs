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
        private readonly OptionsMultiplexer _options;
        private readonly System.Timers.Timer _gcTimer;

        public VotemuteService(OptionsMultiplexer options)
        {
            _options = options;
            _gcTimer = new()
            {
                AutoReset = true,
                Enabled = false,
                Interval = TimeSpan.FromMinutes(15).TotalMilliseconds,
            };
            _gcTimer.Elapsed += (_, _) =>
            {
                _ = Task.Run(async () => await GarbageCollectStatuses());
            };
            _gcTimer.Start();
        }

        protected override async ValueTask OnReactionAdded(ReactionAddedEventArgs e)
        {
            if (e.MessageId is Snowflake messageId && e.ChannelId is Snowflake channelId && e.Member is IMember addingMember)
            {
                VotemuteOptions options = _options.Get<VotemuteOptions>(OptionsAccessibility.Internal, null, e.GuildId) ?? new();
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

                    await AddMuteReaction(addingMember, message);
                }
            }
        }

        private async ValueTask AddMuteReaction(IMember addingMember, IMessage message, CancellationToken cancellationToken = default)
        {
            Task<IMember?> targetMemberTask = Bot.GetOrFetchMember(addingMember.GuildId, message.Author.Id);

            VotemuteMessageStatus newStatus;
            if (MessageHorseCounter.TryGetValue(message.Id, out VotemuteMessageStatus? nullableStatus) && nullableStatus is VotemuteMessageStatus status)
            {
                newStatus = status;
            }
            else
            {
                newStatus = new()
                {
                    CreationTimestamp = message.CreatedAt()
                };
            }

            bool addingMemberHasNotReactedBefore = newStatus.Reactors.Add(addingMember.Id);
            bool addingMemberIsNotSender = message.Author.Id != addingMember.Id;
            bool messageNotOld = !newStatus.Old;
            if (addingMemberHasNotReactedBefore && addingMemberIsNotSender && messageNotOld)
            {
                MessageHorseCounter[message.Id] = newStatus;

                VotemuteOptions options = _options.Get<VotemuteOptions>(OptionsAccessibility.Internal, null, addingMember.GuildId) ?? new();
                if (newStatus.Reactors.Count >= options.NumReactions)
                {
                    if (await targetMemberTask is IMember targetMember)
                    {
                        await Mute(targetMember, MuteReason.ReachedReactionThreshold, cancellationToken);
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

            if (MessageHorseCounter.TryGetValue(e.MessageId, out VotemuteMessageStatus? nullableStatus) && nullableStatus is VotemuteMessageStatus status)
            {
                if (!status.Old)
                {
                    VotemuteOptions options = _options.Get<VotemuteOptions>(OptionsAccessibility.Internal, null, e.GuildId) ?? new();
                    if (status.Reactors.Count > (options.NumReactions / 2.0) && status.Reactors.Count < options.NumReactions)
                    {
                        await Mute(member, MuteReason.DeletedMessageWithReaction);
                    }
                }
            }
        }

        private async ValueTask Mute(IMember member, MuteReason reason, CancellationToken cancellationToken = default)
        {
            VotemuteOptions options = _options.Get<VotemuteOptions>(OptionsAccessibility.Internal, null, member.GuildId) ?? new();

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

        private async Task GarbageCollectStatuses(CancellationToken cancellationToken = default)
        {
            ConcurrentBag<Snowflake> collectableStatusKeys = [];
            IEnumerable<KeyValuePair<Snowflake, VotemuteMessageStatus>> collectableStatuses = MessageHorseCounter.Where((status) => status.Value.Old);
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
    }
}
