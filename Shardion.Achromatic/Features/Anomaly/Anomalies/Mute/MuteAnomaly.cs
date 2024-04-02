using System;
using System.Collections.Generic;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands.Application;
using Shardion.Achromatic.Configuration;
using Shardion.Achromatic.Features.Anomaly.Shop;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Mute
{
    public class MuteAnomaly : AbstractAnomaly
    {
        public override string Name => GetName();

        public MuteAnomaly(OptionsMultiplexer options, AnomalyDatabaseService db, AnomalyManagerService manager, DiscordBotBase bot, IDiscordApplicationGuildCommandContext context, IMember member, AnomalyRecord record) : base(options, db, manager, bot, context, member, record)
        {
        }

        public override IReadOnlyCollection<AbstractAnomalyAction> GetActions()
        {
            List<AbstractAnomalyAction> actions = [];
            if (Record.Partner == AnomalyType.Channel || Record.Tier == AnomalyTier.RealityThreatening)
            {
                actions.Add(new MuteTwoAction(this, AnomalyTier.Joined));
            }
            if (Record.Partner == AnomalyType.Vandalize || Record.Tier == AnomalyTier.RealityThreatening)
            {
                actions.Add(new BolsterAction(this, AnomalyTier.Joined));
            }
            actions.Add(new MuteAction(this, AnomalyTier.Base));
            actions.Add(new GuardAction(this, AnomalyTier.Base));
            return actions;
        }

        private string GetName()
        {
            if (Record.Tier == AnomalyTier.RealityThreatening)
            {
                return "Boss Final";
            }
            else if (Record.Tier == AnomalyTier.Joined)
            {
                if (Record.Partner == AnomalyType.Channel)
                {
                    return "Freeze The World";
                }
                else if (Record.Partner == AnomalyType.Vandalize)
                {
                    return "Realize";
                }
            }
            return "Demon God";
        }
    }
}
