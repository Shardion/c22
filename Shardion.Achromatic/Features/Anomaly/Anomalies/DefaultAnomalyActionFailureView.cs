using System;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies
{
    public class DefaultAnomalyActionFailureView : ViewBase
    {
        public AbstractAnomalyAction Action { get; }

        public DefaultAnomalyActionFailureView(AbstractAnomalyAction action) : base(BuildMessage(action))
        {
            Action = action;
        }

        private static Action<LocalMessageBase> BuildMessage(AbstractAnomalyAction action)
        {
            LocalEmbed embed = new()
            {
                Title = "An Anomaly has failed to act!",
                Description = $"<@{action.Anomaly.Member.Id}> has failed **{action.Name}**!",
                Color = new(TierProperties.GetForTier(action.Tier).Color),
            };

            return (message) =>
            {
                message.AddEmbed(embed);
            };
        }
    }
}
