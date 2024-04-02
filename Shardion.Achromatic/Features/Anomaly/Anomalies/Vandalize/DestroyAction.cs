using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Vandalize
{
    public class DestroyAction : AbstractAnomalyAction
    {
        public override string Name => "Destroy ɑ";
        public override string Description => "Deletes any recent message in the current channel.";

        public override int Cost => TierProperties.GetForTier(Tier).ActionCost;
        public override int Payout => TierProperties.GetForTier(Tier).ActionPayout;
        public override int ChargeSeconds => TierProperties.GetForTier(Tier).ActionSeconds;
        public override bool ChargeNeeded => true;

        public virtual bool CanDeleteOldMessages => false;
        public virtual bool LockToCurrentChannel => true;

        public DestroyAction(AbstractAnomaly anomaly, AnomalyTier tier) : base(anomaly, tier)
        {
        }

        public override AbstractReturnableView? GetPreparationView(Func<Task>? returnTo)
        {
            if (!Anomaly.Manager.MessageSelections.ContainsKey(Anomaly.Member.Id))
            {
                return new DestroyActionSelectMessageView(returnTo, this);
            }

            if (!Anomaly.Manager.MessageSelections.TryGetValue(Anomaly.Member.Id, out IMessage? nullableMessage) || nullableMessage is not IMessage message)
            {
                return new DestroyActionSelectMessageView(returnTo, this);
            }

            if (message.CreatedAt().AddDays(15) < DateTimeOffset.UtcNow && !CanDeleteOldMessages)
            {
                return new DestroyActionSelectMessageView(returnTo, this);
            }

            if (message.ChannelId != Anomaly.Context.ChannelId && LockToCurrentChannel)
            {
                return new DestroyActionSelectMessageView(returnTo, this);
            }

            return null;
        }

        public override async Task Complete()
        {
            await Anomaly.Manager.MessageSelections[Anomaly.Member.Id].DeleteAsync();
        }
    }
}
