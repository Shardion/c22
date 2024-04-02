using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Hosting;
using Disqord.Gateway;
using Shardion.Achromatic.Configuration;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies
{
    public class AnomalyActionCancelService : DiscordBotService
    {
        public AnomalyManagerService Manager { get; }
        public AnomalyOptions AnomalyOptions { get; }
        public OptionsMultiplexer Options { get; }

        public ConcurrentDictionary<Snowflake, int> MessageHorseCounter = [];

        public AnomalyActionCancelService(AnomalyManagerService manager, OptionsMultiplexer opt)
        {
            Manager = manager;
            Options = opt;
            AnomalyOptions = opt.Get<AnomalyOptions>(OptionsAccessibility.NoOne, null, null) ?? new();
        }

        protected override ValueTask OnReactionAdded(ReactionAddedEventArgs e)
        {
            AnomalyOptions opt = Options.Get<AnomalyOptions>(OptionsAccessibility.NoOne, null, null) ?? new();
            if (e.Emoji.Name == opt.Emoji)
            {
                if (!MessageHorseCounter.ContainsKey(e.MessageId))
                {
                    MessageHorseCounter[e.MessageId] = 1;
                }
                else
                {
                    MessageHorseCounter[e.MessageId] += 1;
                }

                if (e.Member is not null)
                {
                    if (Manager.MessageActionStatus.TryGetValue(e.MessageId, out ActionStatus? nullableStatus) && nullableStatus is ActionStatus status)
                    {
                        status.Users.Add(e.Member);
                    }
                }

                if (MessageHorseCounter[e.MessageId] >= AnomalyOptions.RequiredHorsesToCancelAction)
                {
                    if (Manager.MessageActionStatus[e.MessageId].Status != ActionCompletionStatus.Successful)
                    {
                        Manager.MessageActionStatus[e.MessageId].Status = ActionCompletionStatus.Failed;
                    }
                }
            }
            return ValueTask.CompletedTask;
        }
    }
}
