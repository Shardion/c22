using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;
using LiteDB;
using Shardion.Achromatic.Features.Anomaly.Anomalies;
using Shardion.Achromatic.Features.Anomaly.Shop;

namespace Shardion.Achromatic.Features.Anomaly
{
    public class JoinTogetherTwoAnomaliesView : ViewBase
    {
        public AbstractAnomaly StartingMember { get; }
        public AbstractAnomaly OtherMember { get; }

        public bool StartingMemberAccepted { get; set; }
        public bool OtherMemberAccepted { get; set; }

        public JoinTogetherTwoAnomaliesView(AbstractAnomaly startingMember, AbstractAnomaly otherMember) : base(BuildMessage(startingMember, otherMember))
        {
            StartingMember = startingMember;
            OtherMember = otherMember;
        }

        // Has to be instance so disqord picks up on it
        // Needs arg so disqord can call it
#pragma warning disable CA1822, IDE0060

        [Button(Label = "Join!", Style = LocalButtonComponentStyle.Success)]
        public async ValueTask Confirm(ButtonEventArgs e)
        {
            if (e.AuthorId == StartingMember.Member.Id)
            {
                StartingMemberAccepted = true;
            }
            if (e.AuthorId == OtherMember.Member.Id)
            {
                OtherMemberAccepted = true;
            }
            if (StartingMemberAccepted && OtherMemberAccepted)
            {
                e.Button.IsDisabled = true;
                await StartingMember.Database.UpgradeAnomaly(StartingMember, OtherMember, null);

                ILiteCollection<AnomalyStatisticsRecord> statsRecords = StartingMember.Database.Database.GetCollection<AnomalyStatisticsRecord>("anomalyStats");
                AnomalyStatisticsRecord startingStatsRecord = AnomalyDatabaseService.FindOrCreateStatisticsRecord(statsRecords, StartingMember.Member);
                AnomalyStatisticsRecord otherStatsRecord = AnomalyDatabaseService.FindOrCreateStatisticsRecord(statsRecords, OtherMember.Member);
                startingStatsRecord.TimesFused++;
                otherStatsRecord.TimesFused++;
                statsRecords.Update(startingStatsRecord);
                statsRecords.Update(otherStatsRecord);
            }
        }

#pragma warning restore CA1822

        private static Action<LocalMessageBase> BuildMessage(AbstractAnomaly startingMember, AbstractAnomaly otherMember)
        {
            LocalEmbed embed = new()
            {
                Title = "Join together",
                Description = $"""
                Joining together increases your skillset based on the Anomaly you joined with, but is *irreversible!*
                {otherMember.Member.Mention} and {startingMember.Member.Mention}, do you both want to join together?
                """,
                Color = new(TierProperties.GetForTier(startingMember.Record.Tier).Color),
            };

            return (message) =>
            {
                message.AddEmbed(embed);
                message.AllowedMentions = new(new()
                {
                    UserIds = new([startingMember.Member.Id,otherMember.Member.Id]),
                });
            };
        }
    }
}
