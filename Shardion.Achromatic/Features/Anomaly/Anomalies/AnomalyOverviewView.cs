using System;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies
{
    public class AnomalyOverviewView : ViewBase
    {
        public AnomalyOverviewView(AnomalyRecord record) : base(BuildMessage(record))
        {
        }

        private static Action<LocalMessageBase> BuildMessage(AnomalyRecord record)
        {
            LocalEmbed embed = new()
            {
                Title = "Use an Anomaly Action",
                Description = "Select an Anomaly Action from the list below to use it!",
                Color = new(TierProperties.GetForTier(record.Tier).Color),
            };
            LocalEmbedField ppField = new()
            {
                Name = "Play Power",
                Value = new($"{record.PlayPower}/6<:pp:1218384877951516673>"),
                IsInline = true,
            };
            embed.AddField(ppField);

            return (message) =>
            {
                message.AddEmbed(embed);
            };
        }
    }
}
