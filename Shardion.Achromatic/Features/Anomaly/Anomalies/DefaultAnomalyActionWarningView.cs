using System;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies
{
    public class DefaultAnomalyActionWarningView : ViewBase
    {
        public AbstractAnomalyAction Action { get; }

        public DefaultAnomalyActionWarningView(AbstractAnomalyAction action) : base(BuildMessage(action))
        {
            Action = action;
        }

        private static Action<LocalMessageBase> BuildMessage(AbstractAnomalyAction action)
        {
            AnomalyOptions opt = action.Anomaly.Options.Get<AnomalyOptions>(Configuration.OptionsAccessibility.NoOne, null, null) ?? new();
            LocalEmbed embed = new()
            {
                Title = "An Anomaly is acting!",
                Description = $"<@{action.Anomaly.Member.Id}> is charging **{action.Name}**, and will complete **<t:{DateTimeOffset.UtcNow.AddSeconds(action.ChargeSeconds).ToUnixTimeSeconds()}:R>!**\n{opt.RequiredHorsesToCancelAction} 🐴 reactions on this message are needed to stop them!",
                Color = new(TierProperties.GetForTier(action.Tier).Color),
            };

            return (message) =>
            {
                message.AddEmbed(embed);
            };
        }
    }
}
