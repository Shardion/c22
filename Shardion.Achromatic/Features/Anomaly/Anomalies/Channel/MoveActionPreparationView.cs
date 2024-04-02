using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Shardion.Achromatic.Extensions;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Channel
{
    public class MoveActionPreparationView : AbstractReturnableView
    {
        public MoveAction Action { get; }
        public ISnowflakeEntity? TargetChannel { get; set; }
        public ISnowflakeEntity? TargetCategory { get; set; }

        public MoveActionPreparationView(Func<Task>? returnTo, MoveAction action) : base(returnTo, BuildMessage(action))
        {
            Action = action;
        }

        // Has to be instance so disqord picks up on it
        // Needs arg so disqord can call it
#pragma warning disable CA1822, IDE0060

        [Selection(ChannelTypes = [ChannelType.Text, ChannelType.Voice], Type = SelectionComponentType.Channel, Placeholder = "Select channel to move")]
        public ValueTask SelectChannel(SelectionEventArgs e)
        {
            TargetChannel = e.SelectedEntities[0];
            return ValueTask.CompletedTask;
        }

        [Selection(ChannelTypes = [ChannelType.Category], Type = SelectionComponentType.Channel, Placeholder = "Select category to move to")]
        public ValueTask SelectCategory(SelectionEventArgs e)
        {
            TargetCategory = e.SelectedEntities[0];
            return ValueTask.CompletedTask;
        }

        [Button(Label = "Confirm", Style = LocalButtonComponentStyle.Success)]
        public async ValueTask Confirm(ButtonEventArgs e)
        {
            if (e.GuildId is not Snowflake gid || TargetChannel is null)
            {
                return;
            }
            Action.SelectedChannel = TargetChannel;
            Action.SelectedCategory = TargetCategory;
            if (await Action.Anomaly.Bot.GetOrFetchChannel(gid, TargetChannel.Id) is IChannel channel)
            {
                Action.SelectedChannelType = channel.Type;
            }
            if (ReturnTo is not null)
            {
                _ = Task.Run(async () => await ReturnTo());
            }
            return;
        }

#pragma warning restore CA1822

        private static Action<LocalMessageBase> BuildMessage(MoveAction action)
        {
            LocalEmbed embed = new()
            {
                Title = "Move a channel",
                Description = "Select a channel to move, and a category to move it to.",
                Color = new(TierProperties.GetForTier(action.Tier).Color),
            };

            return (message) =>
            {
                message.AddEmbed(embed);
            };
        }
    }
}
