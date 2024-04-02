using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;
using Shardion.Achromatic.Extensions;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Vandalize
{
    public class DestroyActionSelectMessageView : AbstractReturnableView
    {
        public DestroyAction Action { get; }

        public DestroyActionSelectMessageView(Func<Task>? returnTo, DestroyAction action) : base(returnTo, BuildMessage(action))
        {
            Action = action;
        }

        // Has to be instance so disqord picks up on it
        // Needs arg so disqord can call it
#pragma warning disable CA1822, IDE0060

        [Button(Label = "I've selected a message", Style = LocalButtonComponentStyle.Success)]
        public ValueTask Confirm(ButtonEventArgs e)
        {
            if (!Action.Anomaly.Manager.MessageSelections.TryGetValue(Action.Anomaly.Member.Id, out IMessage? nullableMessage) || nullableMessage is not IMessage message)
            {
                return ValueTask.CompletedTask;
            }

            if (message.CreatedAt().AddDays(15) < DateTimeOffset.UtcNow && !Action.CanDeleteOldMessages)
            {
                return ValueTask.CompletedTask;
            }

            if (message.ChannelId != e.ChannelId && Action.LockToCurrentChannel)
            {
                return ValueTask.CompletedTask;
            }
            
            if (ReturnTo is not null)
            {
                _ = Task.Run(async () => await ReturnTo());
            }
            return ValueTask.CompletedTask;
        }

#pragma warning restore CA1822

        private static Action<LocalMessageBase> BuildMessage(DestroyAction action)
        {
            LocalEmbed embed = new()
            {
                Title = "Select a message",
                Description = "To use Destroy, you need to select a message to delete. Right-click your target message, hover over the `Apps` entry, and then click `Select Message`.",
                Footer = new(new LocalEmbedFooter()
                {
                    Text = "Tip: Didn't work? Make sure the active tier of Destroy can delete your message!"
                }),
                Color = new(TierProperties.GetForTier(action.Tier).Color),
            };

            return (message) =>
            {
                message.AddEmbed(embed);
            };
        }
    }
}
