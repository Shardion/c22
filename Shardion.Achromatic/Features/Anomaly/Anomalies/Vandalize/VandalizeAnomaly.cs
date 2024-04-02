using System;
using System.Collections.Generic;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands.Application;
using Disqord.Extensions.Interactivity.Menus;
using Shardion.Achromatic.Configuration;
using Shardion.Achromatic.Features.Anomaly.Shop;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Vandalize
{
    public class VandalizeAnomaly : AbstractAnomaly
    {
        public override string Name => GetName();

        public VandalizeAnomaly(OptionsMultiplexer options, AnomalyDatabaseService db, AnomalyManagerService manager, DiscordBotBase bot, IDiscordApplicationGuildCommandContext context, IMember member, AnomalyRecord record) : base(options, db, manager, bot, context, member, record)
        {
        }

        public override IReadOnlyCollection<AbstractAnomalyAction> GetActions()
        {
            List<AbstractAnomalyAction> actions = [];
            if (Record.Partner == AnomalyType.Mute || Record.Tier == AnomalyTier.RealityThreatening)
            {
                actions.Add(new PaintTwoAction(this, AnomalyTier.Joined));
            }
            if (Record.Partner == AnomalyType.Channel || Record.Tier == AnomalyTier.RealityThreatening)
            {
                actions.Add(new DestroyTwoAction(this, AnomalyTier.Joined));
            }
            actions.Add(new DestroyAction(this, AnomalyTier.Base));
            actions.Add(new PaintAction(this, AnomalyTier.Base));
            return actions;
        }

        private string GetName()
        {
            if (Record.Tier == AnomalyTier.RealityThreatening)
            {
                return "End Of Story";
            }
            else if (Record.Tier == AnomalyTier.Joined)
            {
                if (Record.Partner == AnomalyType.Mute)
                {
                    return "Legend";
                }
                else if (Record.Partner == AnomalyType.Channel)
                {
                    return "The Deed";
                }
            }
            return "The Ruler";
        }
    }
}
