using Disqord.Bot.Hosting;
using Shardion.Achromatic.Common.Timers;
using LiteDB;
using System.Threading.Tasks;
using Disqord.Gateway;

namespace Shardion.Achromatic.Features.TimerSentinel
{
    public class TimerSentinelService : DiscordBotService
    {
        private readonly LiteDatabase _db;

        public TimerSentinelService(LiteDatabase db)
        {
            _db = db;
        }

        protected override ValueTask OnReady(ReadyEventArgs e)
        {
            AchromaticTimer.StartOrResumeTimerControllers(_db);
            return ValueTask.CompletedTask;
        }
    }
}
