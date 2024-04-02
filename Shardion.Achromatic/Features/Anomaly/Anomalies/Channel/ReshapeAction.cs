using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;
using LiteDB;
using Shardion.Achromatic.Features.Anomaly.Shop;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Channel
{
    public class ReshapeAction : AbstractAnomalyAction
    {
        public override string Name => "Reshape ɑ";
        public override string Description => "Grants permission editing on an existing channel.";

        public override int Cost => TierProperties.GetForTier(Tier).ActionCost;
        public override int Payout => TierProperties.GetForTier(Tier).ActionPayout;
        public override int ChargeSeconds => TierProperties.GetForTier(Tier).ActionSeconds;
        public override bool ChargeNeeded => true;

        public IGuildChannel? SelectedChannel { get; set; }

        public ReshapeAction(AbstractAnomaly anomaly, AnomalyTier tier) : base(anomaly, tier)
        {
        }

        public override async Task Complete()
        {
            if (SelectedChannel is null)
            {
                return;
            }
            await SelectedChannel.SetOverwriteAsync(new LocalOverwrite()
            {
                TargetId = Anomaly.Member.Id,
                TargetType = new(OverwriteTargetType.Member),
                Permissions = new(new OverwritePermissions(Permissions.ManageRoles, Permissions.None)),
            });

            ILiteCollection<AnomalyStatisticsRecord> records = Anomaly.Database.Database.GetCollection<AnomalyStatisticsRecord>("anomalyStats");
            AnomalyStatisticsRecord record = AnomalyDatabaseService.FindOrCreateStatisticsRecord(records, Anomaly.Member);
            record.ChannelsReshaped++;
            records.Update(record);
        }

        public override AbstractReturnableView? GetPreparationView(Func<Task>? returnTo)
        {
            return new ReshapeActionPreparationView(returnTo, this);
        }
    }
}
