using System.Threading.Tasks;
using System.Collections.Generic;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Gateway;
using Disqord.Rest;
using System;

namespace Shardion.Achromatic.Common.Menus
{
    public abstract class LoudRestrictedMenuBase : DefaultMenuBase
    {
        public static readonly string[] Responses =
        [
            "No",
            "Nah",
            "Not happening",
        ];

        public ISet<Snowflake> AllowedUsers = new HashSet<Snowflake>();

        public LoudRestrictedMenuBase(ViewBase view) : base(view)
        {
        }

        protected override async ValueTask<bool> CheckInteractionAsync(InteractionReceivedEventArgs e)
        {
            if (AllowedUsers.Count == 0 || AllowedUsers.Contains(e.AuthorId))
            {
                return true;
            }

            var message = new LocalInteractionMessageResponse()
                                .WithIsEphemeral(true)
                                .WithContent(Responses[Random.Shared.Next(Responses.Length)]);

            await e.Interaction.Response().SendMessageAsync(message);
            return false;
        }
    }
}
