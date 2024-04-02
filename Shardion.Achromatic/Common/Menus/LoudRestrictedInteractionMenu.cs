using System.Threading.Tasks;
using System.Threading;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;
using System.Collections.Generic;

namespace Shardion.Achromatic.Common.Menus
{
    public class LoudRestrictedInteractionMenu : LoudRestrictedMenuBase
    {
        public IUserInteraction Interaction { get; }

        public LoudRestrictedInteractionMenu(ViewBase view, IUserInteraction interaction, IReadOnlyCollection<Snowflake>? allowedMembers = null) : base(view)
        {
            Interaction = interaction;
            if (allowedMembers is not null)
            {
                
            }
            else
            {

            }
        }

        public override LocalMessageBase CreateLocalMessage()
        {
            return new LocalInteractionMessageResponse();
        }

        protected override async Task<IUserMessage> SendLocalMessageAsync(LocalMessageBase message, CancellationToken cancellationToken)
        {
            var response = Interaction.Response();
            var interactionMessageResponse = (message as LocalInteractionMessageResponse)!;
            if (!response.HasResponded)
            {
                await response.SendMessageAsync(interactionMessageResponse, cancellationToken: cancellationToken).ConfigureAwait(false);
                return await Interaction.Followup().FetchResponseAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            return await Interaction.Followup().SendAsync(interactionMessageResponse, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}
