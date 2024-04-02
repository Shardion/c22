using System.Collections.Generic;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands.Application;
using Shardion.Achromatic.Configuration;
using Shardion.Achromatic.Features.Anomaly.Shop;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies
{
    public abstract class AbstractAnomaly
    {
        public abstract string Name { get; }
        public IMember Member { get; }
        public AnomalyRecord Record { get; }

        public AnomalyDatabaseService Database { get; }
        public OptionsMultiplexer Options { get; }
        public AnomalyManagerService Manager { get; }
        public DiscordBotBase Bot { get; }
        public IDiscordApplicationGuildCommandContext Context { get; }

        public AbstractAnomaly(OptionsMultiplexer options, AnomalyDatabaseService db, AnomalyManagerService manager, DiscordBotBase bot, IDiscordApplicationGuildCommandContext context, IMember member, AnomalyRecord record)
        {
            Options = options;
            Database = db;
            Manager = manager;
            Bot = bot;
            Context = context;
            Member = member;
            Record = record;
        }

        public abstract IReadOnlyCollection<AbstractAnomalyAction> GetActions();
    }
}
