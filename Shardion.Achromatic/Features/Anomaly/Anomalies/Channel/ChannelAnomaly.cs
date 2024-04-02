using System;
using System.Collections.Generic;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands.Application;
using Disqord.Extensions.Interactivity.Menus;
using Shardion.Achromatic.Configuration;
using Shardion.Achromatic.Features.Anomaly.Shop;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Channel
{
    public class ChannelAnomaly : AbstractAnomaly
    {
        public override string Name => GetName();

        public ChannelAnomaly(OptionsMultiplexer options, AnomalyDatabaseService db, AnomalyManagerService manager, DiscordBotBase bot, IDiscordApplicationGuildCommandContext context, IMember member, AnomalyRecord record) : base(options, db, manager, bot, context, member, record)
        {
        }

        public override IReadOnlyCollection<AbstractAnomalyAction> GetActions()
        {
            List<AbstractAnomalyAction> actions = [];
            if (Record.Partner == AnomalyType.Mute || Record.Tier == AnomalyTier.RealityThreatening)
            {
                actions.Add(new MoveTwoAction(this, AnomalyTier.Joined));
            }
            if (Record.Partner == AnomalyType.Vandalize || Record.Tier == AnomalyTier.RealityThreatening)
            {
                actions.Add(new ReshapeAction(this, AnomalyTier.Joined));
            }
            actions.Add(new MoveAction(this, AnomalyTier.Base));
            actions.Add(new ConstructAction(this, AnomalyTier.Base));
            return actions;
        }

        private string GetName()
        {
            if (Record.Tier == AnomalyTier.RealityThreatening)
            {
                return "Harmony";
            }
            else if (Record.Tier == AnomalyTier.Joined)
            {
                if (Record.Partner == AnomalyType.Mute)
                {
                    return "Dreamland";
                }
                else if (Record.Partner == AnomalyType.Vandalize)
                {
                    return "Shattered";
                }
            }
            return "Awakened";
        }
    }
}
