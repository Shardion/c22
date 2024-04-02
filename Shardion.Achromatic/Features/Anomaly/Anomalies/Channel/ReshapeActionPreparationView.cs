using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Gateway;
using Shardion.Achromatic.Extensions;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Channel
{
    public class ReshapeActionPreparationView : AbstractReturnableView
    {
        public ReshapeAction Action { get; }
        public IGuildChannel? TargetChannel { get; set; }

        public ReshapeActionPreparationView(Func<Task>? returnTo, ReshapeAction action) : base(returnTo, BuildMessage(action))
        {
            Action = action;
        }

        // Has to be instance so disqord picks up on it
        // Needs arg so disqord can call it
#pragma warning disable CA1822, IDE0060

        [Selection(ChannelTypes = [ChannelType.Text, ChannelType.Voice], Type = SelectionComponentType.Channel, Placeholder = "Select channel to take control of")]
        public async ValueTask SelectChannel(SelectionEventArgs e)
        {
            if (e.GuildId is Snowflake gid)
            {
                TargetChannel = await Action.Anomaly.Bot.GetOrFetchChannel(gid, e.SelectedEntities[0].Id) as IGuildChannel;
            }
        }

        [Button(Label = "Confirm", Style = LocalButtonComponentStyle.Success)]
        public ValueTask Confirm(ButtonEventArgs e)
        {
            if (e.GuildId is not Snowflake gid || TargetChannel is null)
            {
                return ValueTask.CompletedTask;
            }
            Action.SelectedChannel = TargetChannel;
            if (ReturnTo is not null)
            {
                _ = Task.Run(async () => await ReturnTo());
            }
            return ValueTask.CompletedTask;
        }

#pragma warning restore CA1822

        private static Action<LocalMessageBase> BuildMessage(ReshapeAction action)
        {
            LocalEmbed embed = new()
            {
                Title = "Reshape a channel",
                Description = "Select a channel to gain control over.",
                Color = new(TierProperties.GetForTier(action.Tier).Color),
            };

            return (message) =>
            {
                message.AddEmbed(embed);
            };
        }
    }
}
