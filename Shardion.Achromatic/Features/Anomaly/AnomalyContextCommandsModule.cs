using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands.Application;
using Qmmands;
using Shardion.Achromatic.Configuration;
using Shardion.Achromatic.Features.Anomaly.Anomalies;
using Shardion.Achromatic.Features.Anomaly.Shop;

namespace Shardion.Achromatic.Features.Anomaly
{

    public class AnomalyContextCommandsModule : DiscordApplicationGuildModuleBase
    {
        private readonly AnomalyOptions _options;
        private readonly AnomalyDatabaseService _database;
        private readonly AnomalyManagerService _manager;

        public AnomalyContextCommandsModule(OptionsMultiplexer options, AnomalyDatabaseService banker, AnomalyManagerService manager)
        {
            _options = options.Get<AnomalyOptions>(OptionsAccessibility.NoOne, null, null) ?? new();
            _database = banker;
            _manager = manager;
        }

        [MessageCommand("Select Message")]
        [Description("Selects a message to use with an Anomaly Action.")]
        public ValueTask<IResult> SelectMessage(IMessage message)
        {
            _manager.MessageSelections[Context.AuthorId] = message;
            return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
            {
                IsEphemeral = true,
                Content = "Selected this message.",
            }));
        }
    }
}
