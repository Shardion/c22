using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Mute
{
    public class GuardActionPreparationView : AbstractReturnableView
    {
        public GuardAction Action { get; }
        public ISnowflakeEntity? GuardTarget { get; set; }

        public GuardActionPreparationView(Func<Task>? returnTo, GuardAction action) : base(returnTo, BuildMessage(action))
        {
            Action = action;

            SelectionViewComponent targets = new((x) =>
            {
                GuardTarget = x.SelectedEntities[0];
                return ValueTask.CompletedTask;
            })
            {
                Type = SelectionComponentType.User,
                Placeholder = "Select member to guard",
            };

            AddComponent(targets);
        }

        // Has to be instance so disqord picks up on it
        // Needs arg so disqord can call it
#pragma warning disable CA1822, IDE0060

        [Button(Label = "Confirm", Style = LocalButtonComponentStyle.Success)]
        public ValueTask Confirm(ButtonEventArgs e)
        {
            if (GuardTarget is not null)
            {
                Action.GuardTarget = GuardTarget;

                if (ReturnTo is not null)
                {
                    _ = Task.Run(async () => await ReturnTo());
                }
            }
            return ValueTask.CompletedTask;
        }

#pragma warning restore CA1822

        private static Action<LocalMessageBase> BuildMessage(GuardAction action)
        {
            LocalEmbed embed = new()
            {
                Title = "Choose who to guard",
                Description = "Select the member to guard.",
                Color = new(TierProperties.GetForTier(action.Tier).Color),
            };

            return (message) =>
            {
                message.AddEmbed(embed);
            };
        }
    }
}
