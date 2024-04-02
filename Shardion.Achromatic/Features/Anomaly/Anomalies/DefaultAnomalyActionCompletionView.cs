using System;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies
{
    public class DefaultAnomalyActionCompletionView : ViewBase
    {
        public AbstractAnomalyAction Action { get; }

        public DefaultAnomalyActionCompletionView(AbstractAnomalyAction action) : base(BuildMessage(action))
        {
            Action = action;
        }

        private static Action<LocalMessageBase> BuildMessage(AbstractAnomalyAction action)
        {
            LocalEmbed embed = new()
            {
                Title = "An Anomaly has acted!",
                Description = $"<@{action.Anomaly.Member.Id}> performed **{action.Name}**!",
                Color = new(TierProperties.GetForTier(action.Tier).Color),
            };

            return (message) =>
            {
                message.AddEmbed(embed);
            };
        }
    }
}
