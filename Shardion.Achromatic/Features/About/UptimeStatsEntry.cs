using System;
using System.Text;

namespace Shardion.Achromatic.Features.About
{
    public class UptimeStatsEntry : IStatsEntry
    {
        public string Name => "Uptime";

        public string GetStatistic()
        {
            string uptime = FormatTimeSpanAsUptimeString(DateTime.UtcNow.Subtract(Program.StartTime));
            string uptimeSince = new DateTimeOffset(Program.StartTime).ToUnixTimeSeconds().ToString();
            return $"{uptime} (since <t:{uptimeSince}>)";
        }

        private static string FormatTimeSpanAsUptimeString(TimeSpan timeSpan)
        {
            StringBuilder uptimeStringBuilder = new($"{timeSpan.Seconds}s");
            if (timeSpan.Minutes > 0)
            {
                uptimeStringBuilder.Insert(0, $"{timeSpan.Minutes}m ");
            }
            if (timeSpan.Hours > 0)
            {
                uptimeStringBuilder.Insert(0, $"{timeSpan.Hours}h");
            }
            if (timeSpan.Days > 0)
            {
                uptimeStringBuilder.Insert(0, $"{timeSpan.Days % 7}d");
            }
            if (timeSpan.Days > 7)
            {
                uptimeStringBuilder.Insert(0, $"{Convert.ToInt32(MathF.Floor(timeSpan.Days / 7))}w");
            }
            return uptimeStringBuilder.ToString();
        }
    }
}
