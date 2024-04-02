using System;
using System.Threading.Tasks;
using Disqord.Extensions.Interactivity.Menus;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies
{
    public abstract class AbstractAnomalyAction
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract int Cost { get; }
        public abstract int Payout { get; }
        public abstract int ChargeSeconds { get; }
        public abstract bool ChargeNeeded { get; }

        public AnomalyTier Tier { get; }
        public AbstractAnomaly Anomaly { get; }

        public AbstractAnomalyAction(AbstractAnomaly anomaly, AnomalyTier tier)
        {
            Anomaly = anomaly;
            Tier = tier;
        }

        public virtual async Task Charge()
        {
            await Task.Delay(ChargeSeconds * 1000);
        }

        public abstract Task Complete();

        public virtual AbstractReturnableView? GetPreparationView(Func<Task>? returnTo)
        {
            return null;
        }

        public virtual ViewBase GetWarningView()
        {
            return new DefaultAnomalyActionWarningView(this);
        }

        public virtual ViewBase GetCompletionView()
        {
            return new DefaultAnomalyActionCompletionView(this);
        }

        public virtual ViewBase GetFailureView()
        {
            return new DefaultAnomalyActionFailureView(this);
        }
    }
}
