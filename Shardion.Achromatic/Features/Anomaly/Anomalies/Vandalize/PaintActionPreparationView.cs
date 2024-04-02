using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Vandalize
{
    public class PaintActionPreparationView : AbstractReturnableView
    {
        public PaintAction Action { get; }
        public ICollection<ISnowflakeEntity> AddToTargets { get; set; } = [];

        public PaintActionPreparationView(Func<Task>? returnTo, PaintAction action) : base(returnTo, BuildMessage(action))
        {
            Action = action;

            SelectionViewComponent targets = new((x) =>
            {
                AddToTargets = (ICollection<ISnowflakeEntity>)x.SelectedEntities;
                return ValueTask.CompletedTask;
            })
            {
                Type = SelectionComponentType.User,
                MaximumSelectedOptions = action.MaximumTargets,
            };

            AddComponent(targets);
        }

        // Has to be instance so disqord picks up on it
        // Needs arg so disqord can call it
#pragma warning disable CA1822, IDE0060

        [Button(Label = "Confirm", Style = LocalButtonComponentStyle.Success)]
        public ValueTask Confirm(ButtonEventArgs e)
        {
            Action.AddToTargets = (IReadOnlyCollection<ISnowflakeEntity>?)AddToTargets;

            if (ReturnTo is not null)
            {
                _ = Task.Run(async () => await ReturnTo());
            }
            return ValueTask.CompletedTask;
        }

#pragma warning restore CA1822

        private static Action<LocalMessageBase> BuildMessage(PaintAction action)
        {
            LocalEmbed embed = new()
            {
                Title = "Choose who to add to your role",
                Description = "Choose who should be assigned to the new role.",
                Color = new(TierProperties.GetForTier(action.Tier).Color),
                Footer = new(new()
                {
                    Text = "Tip: You'll get to change its name and color later, with /anomaly paint.",
                })
            };

            return (message) =>
            {
                message.AddEmbed(embed);
            };
        }
    }
}
