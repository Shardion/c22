using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Vandalize
{
    public class PaintAction : AbstractAnomalyAction
    {
        public override string Name => "Paint ɑ";
        public override string Description => "Creates a new role and assigns you to it. (Change name and color with `/anomaly paint`.)";

        public override int Cost => TierProperties.GetForTier(Tier).ActionCost;
        public override int Payout => TierProperties.GetForTier(Tier).ActionPayout;
        public override int ChargeSeconds => TierProperties.GetForTier(Tier).ActionSeconds;
        public override bool ChargeNeeded => true;

        public virtual bool LockToStarter => true;
        public virtual int MaximumTargets => 1;

        public IReadOnlyCollection<ISnowflakeEntity>? AddToTargets = null;

        public PaintAction(AbstractAnomaly anomaly, AnomalyTier tier) : base(anomaly, tier)
        {
        }

        public override AbstractReturnableView? GetPreparationView(Func<Task>? returnTo)
        {
            if (!LockToStarter)
            {
                return new PaintActionPreparationView(returnTo, this);
            }
            return base.GetPreparationView(returnTo);
        }

        public override async Task Complete()
        {
            AddToTargets ??= [Anomaly.Member];

            IRole role = await Anomaly.Bot.CreateRoleAsync(Anomaly.Context.GuildId, (a) =>
            {
                a.Name = $"{Anomaly.Member.Name}'s role";
            });

            List<Task> roleTasks = [];
            foreach (ISnowflakeEntity target in AddToTargets)
            {
                roleTasks.Add(Anomaly.Bot.GrantRoleAsync(Anomaly.Context.GuildId, target.Id, role.Id));
            }

            await Anomaly.Database.AddPaintedRole(Anomaly.Member, role.Id);

            await Task.WhenAll(roleTasks);
        }
    }
}
