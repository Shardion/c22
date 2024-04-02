using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands.Application;
using Disqord.Rest;
using Qmmands;
using Shardion.Achromatic.Common.Constants;

namespace Shardion.Achromatic.Features.Poll
{
    public class PollModule : DiscordApplicationGuildModuleBase
    {
        [MessageCommand("Poll")]
        [Description("Makes the bot react to this message with <:yea:1197353562502610945> and <:na:1130605328959017040>.")]
        public async ValueTask<IResult> Poll(IMessage message)
        {
            bool removeReactions = false;
            Task<IReadOnlyList<IUser>> yeaReactions = message.FetchReactionsAsync(EnchEmojis.YEA);
            Task<IReadOnlyList<IUser>> naReactions = message.FetchReactionsAsync(EnchEmojis.NA);
            List<Task<IReadOnlyList<IUser>>> reactions = [yeaReactions, naReactions];
            await Task.WhenAll(reactions);
            await Parallel.ForEachAsync(reactions, async (reactionSet, ct) =>
            {
                if ((await reactionSet).Any((u) => u.Id == Bot.CurrentUser.Id))
                {
                    removeReactions = true;
                }
            });

            if (!removeReactions)
            {
                Task addReactionsTask = Task.Run(async () =>
                {
                    await Task.WhenAll([
                        message.AddReactionAsync(EnchEmojis.YEA),
                        message.AddReactionAsync(EnchEmojis.NA),
                    ]);
                });
                await Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "Poll emojis added. Run Poll on this message again to remove the bot's reactions.",
                });
                await addReactionsTask;
            }
            else
            {
                Task removeReactionsTask = Task.Run(async () =>
                {
                    await Task.WhenAll([
                        message.RemoveOwnReactionAsync(EnchEmojis.YEA),
                        message.RemoveOwnReactionAsync(EnchEmojis.NA),
                    ]);
                });
                await Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "Poll emojis removed. Run Poll on this message again to re-add the bot's reactions.",
                });
                await removeReactionsTask;
            }
            return new Result();
        }
    }
}
