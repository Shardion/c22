using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies
{
    public abstract class AbstractReturnableView : ViewBase
    {
        protected AbstractReturnableView(Func<Task>? returnTo, Action<LocalMessageBase>? messageTemplate) : base(messageTemplate)
        {
            ReturnTo = returnTo;
        }

        public Func<Task>? ReturnTo { get; set; }
    }
}
