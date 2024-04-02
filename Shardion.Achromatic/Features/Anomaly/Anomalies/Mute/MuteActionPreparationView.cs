using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Mute
{
    public class MuteActionPreparationView : AbstractReturnableView
    {
        public MuteAction Action { get; }
        public ICollection<ISnowflakeEntity> MuteTargets { get; set; } = [];

        public MuteActionPreparationView(Func<Task>? returnTo, MuteAction action) : base(returnTo, BuildMessage(action))
        {
            Action = action;

            SelectionViewComponent targets = new((x) =>
            {
                MuteTargets = (ICollection<ISnowflakeEntity>)x.SelectedEntities;
                return ValueTask.CompletedTask;
            })
            {
                Type = SelectionComponentType.User,
                MaximumSelectedOptions = action.MaxMuteTargets,
            };

            if (action.MaxMuteTargets > 1)
            {
                targets.Placeholder = "Select target members";
            }
            else
            {
                targets.Placeholder = "Select target member";
            }

            AddComponent(targets);
        }

        // Has to be instance so disqord picks up on it
        // Needs arg so disqord can call it
#pragma warning disable CA1822, IDE0060

        [Button(Label = "Confirm", Style = LocalButtonComponentStyle.Success)]
        public ValueTask Confirm(ButtonEventArgs e)
        {
            Action.MuteTargets = (IReadOnlyCollection<ISnowflakeEntity>?)MuteTargets;

            if (ReturnTo is not null)
            {
                _ = Task.Run(async () => await ReturnTo());
            }
            return ValueTask.CompletedTask;
        }

#pragma warning restore CA1822

        private static Action<LocalMessageBase> BuildMessage(MuteAction action)
        {
            LocalEmbed embed = new()
            {
                Title = "Choose who to mute",
                Color = new(TierProperties.GetForTier(action.Tier).Color),
            };
            if (action.MaxMuteTargets > 1)
            {
                embed.Description = "Select the members who should be muted.";
            }
            else
            {
                embed.Description = "Select the member who should be muted.";
            }

            return (message) =>
            {
                message.AddEmbed(embed);
            };
        }
    }
}
