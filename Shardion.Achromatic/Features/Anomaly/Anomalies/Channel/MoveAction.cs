using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;
using LiteDB;
using Shardion.Achromatic.Extensions;
using Shardion.Achromatic.Features.Anomaly.Shop;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Channel
{
    public class MoveAction : AbstractAnomalyAction
    {
        public override string Name => "Move ɑ";
        public override string Description => "Moves a channel without syncing permissions.";

        public override int Cost => TierProperties.GetForTier(Tier).ActionCost;
        public override int Payout => TierProperties.GetForTier(Tier).ActionPayout;
        public override int ChargeSeconds => TierProperties.GetForTier(Tier).ActionSeconds;
        public override bool ChargeNeeded => true;

        public virtual bool SyncPermissions => false;

        public ISnowflakeEntity? SelectedChannel { get; set; }
        public ISnowflakeEntity? SelectedCategory { get; set; }
        public ChannelType? SelectedChannelType { get; set; }

        public MoveAction(AbstractAnomaly anomaly, AnomalyTier tier) : base(anomaly, tier)
        {
        }

        public override async Task Complete()
        {
            if (SelectedChannel is null || SelectedCategory is null || SelectedChannelType is null)
            {
                return;
            }

            ILiteCollection<AnomalyStatisticsRecord> records = Anomaly.Database.Database.GetCollection<AnomalyStatisticsRecord>("anomalyStats");
            AnomalyStatisticsRecord record = AnomalyDatabaseService.FindOrCreateStatisticsRecord(records, Anomaly.Member);

            List<LocalOverwrite>? overwrites = null;
            if (SyncPermissions)
            {
                if (await Anomaly.Bot.GetOrFetchChannel(Anomaly.Context.GuildId, SelectedCategory.Id) is ICategoryChannel category)
                {
                    overwrites = [];
                    foreach (IOverwrite overwrite in category.Overwrites)
                    {
                        overwrites.Add(LocalOverwrite.CreateFrom(overwrite));
                    }
                }
                record.PermissionsSynced++;
            }
            record.ChannelsMoved++;
            records.Update(record);

            switch (SelectedChannelType)
            {
                default:
                    
                    if (overwrites is not null)
                    {
                    }
                    await Anomaly.Bot.ModifyTextChannelAsync(SelectedChannel.Id, (x) =>
                    {
                        x.CategoryId = SelectedCategory.Id;
                        if (overwrites is not null)
                        {
                            x.Overwrites = new(overwrites);
                        }
                        
                    });
                    records.Update(record);
                    break;
                case ChannelType.Voice:
                    await Anomaly.Bot.ModifyVoiceChannelAsync(SelectedChannel.Id, (x) =>
                    {
                        x.CategoryId = SelectedCategory.Id;
                        if (overwrites is not null)
                        {
                            x.Overwrites = new(overwrites);
                        }
                    });
                    break;
            }
        }

        public override AbstractReturnableView? GetPreparationView(Func<Task>? returnTo)
        {
            return new MoveActionPreparationView(returnTo, this);
        }
    }
}
