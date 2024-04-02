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
    public class JoinTogetherThreeAnomaliesView : ViewBase
    {
        public AbstractAnomaly StartingMember { get; }
        public AbstractAnomaly SecondMember { get; }
        public AbstractAnomaly ThirdMember { get; }

        public bool StartingMemberAccepted { get; set; }
        public bool SecondMemberAccepted { get; set; }
        public bool ThirdMemberAccepted { get; set; }

        public JoinTogetherThreeAnomaliesView(AbstractAnomaly startingMember, AbstractAnomaly secondMember, AbstractAnomaly thirdMember) : base(BuildMessage(startingMember, secondMember, thirdMember))
        {
            StartingMember = startingMember;
            SecondMember = secondMember;
            ThirdMember = thirdMember;
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
            if (e.AuthorId == SecondMember.Member.Id)
            {
                SecondMemberAccepted = true;
            }
            if (e.AuthorId == ThirdMember.Member.Id)
            {
                ThirdMemberAccepted = true;
            }
            if (StartingMemberAccepted && SecondMemberAccepted && ThirdMemberAccepted)
            {
                e.Button.IsDisabled = true;
                await StartingMember.Database.UpgradeAnomaly(StartingMember, SecondMember, ThirdMember);

                ILiteCollection<AnomalyStatisticsRecord> statsRecords = StartingMember.Database.Database.GetCollection<AnomalyStatisticsRecord>("anomalyStats");
                AnomalyStatisticsRecord startingStatsRecord = AnomalyDatabaseService.FindOrCreateStatisticsRecord(statsRecords, StartingMember.Member);
                AnomalyStatisticsRecord secondStatsRecord = AnomalyDatabaseService.FindOrCreateStatisticsRecord(statsRecords, SecondMember.Member);
                AnomalyStatisticsRecord thirdStatsRecord = AnomalyDatabaseService.FindOrCreateStatisticsRecord(statsRecords, ThirdMember.Member);
                startingStatsRecord.TimesFused++;
                secondStatsRecord.TimesFused++;
                thirdStatsRecord.TimesFused++;
                statsRecords.Update(startingStatsRecord);
                statsRecords.Update(secondStatsRecord);
                statsRecords.Update(thirdStatsRecord);
            }
        }

#pragma warning restore CA1822

        private static Action<LocalMessageBase> BuildMessage(AbstractAnomaly startingMember, AbstractAnomaly secondMember, AbstractAnomaly thirdMember)
        {
            LocalEmbed embed = new()
            {
                Title = "Join together",
                Description = $"""
                Joining together increases your skillset based on the Anomaly you joined with, but is *irreversible!*
                {startingMember.Member.Mention}, {secondMember.Member.Mention}, and {thirdMember.Member.Mention}, do you three want to **triple-fuse???**
                """,
                Color = new(TierProperties.GetForTier(startingMember.Record.Tier).Color),
            };

            return (message) =>
            {
                message.AddEmbed(embed);
                message.AllowedMentions = new(new()
                {
                    UserIds = new([startingMember.Member.Id,secondMember.Member.Id,thirdMember.Member.Id]),
                });
            };
        }
    }
}
