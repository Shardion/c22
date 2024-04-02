using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies.Channel
{
    public class ConstructActionPreparationView : AbstractReturnableView
    {
        private static readonly HashSet<char> AllowedChars =
        [
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            '1', '2', '3', '4', '5', '6', '7', '8', '9', '0',
            '-', '_',
        ];

        public ConstructAction Action { get; }
        public IReadOnlyCollection<ISnowflakeEntity> ModeratorUsers { get; set; } = new List<ISnowflakeEntity>(0);
        public IReadOnlyCollection<ISnowflakeEntity> BannedUsers { get; set; } = new List<ISnowflakeEntity>(0);

        public ConstructActionPreparationView(Func<Task>? returnTo, ConstructAction action) : base(returnTo, BuildMessage(action))
        {
            Action = action;

            SelectionViewComponent allowed = new((x) =>
            {
                ModeratorUsers = x.SelectedEntities;
                return ValueTask.CompletedTask;
            })
            {
                Type = SelectionComponentType.User,
                MinimumSelectedOptions = 0,
                MaximumSelectedOptions = 25,
                Placeholder = "Select admins",
            };

            SelectionViewComponent denied = new((x) =>
            {
                BannedUsers = x.SelectedEntities;
                return ValueTask.CompletedTask;
            })
            {
                Type = SelectionComponentType.User,
                MinimumSelectedOptions = 0,
                MaximumSelectedOptions = 25,
                Placeholder = "Select banned members",
            };

            AddComponent(allowed);
            AddComponent(denied);
        }

// Has to be instance so disqord picks up on it
// Needs arg so disqord can call it
#pragma warning disable CA1822,IDE0060

        [Button(Label = "Confirm", Style = LocalButtonComponentStyle.Success)]
        public ValueTask Confirm(ButtonEventArgs e)
        {
            OverwritePermissions ownerPerms = OverwritePermissions.None.Allow(Permissions.ManageChannels | Permissions.ManageRoles | Permissions.ManageMessages);
            OverwritePermissions modPerms = OverwritePermissions.None.Allow(Permissions.ManageChannels | Permissions.ManageRoles | Permissions.ManageMessages);
            OverwritePermissions bannedPerms = OverwritePermissions.None.Deny(Permissions.ViewChannels);

            Dictionary<Snowflake, OverwriteContainer> overwrites = [];

            foreach (ISnowflakeEntity ent in BannedUsers)
            {
                overwrites[ent.Id] = new()
                {
                    Overwrite = new()
                    {
                        TargetId = new(ent.Id),
                        TargetType = new(OverwriteTargetType.Member),
                        Permissions = new(bannedPerms),
                    },
                    Intention = OverwriteIntention.Negative,
                };
            }

            foreach (ISnowflakeEntity ent in ModeratorUsers)
            {
                overwrites[ent.Id] = new()
                {
                    Overwrite = new()
                    {
                        TargetId = new(ent.Id),
                        TargetType = new(OverwriteTargetType.Member),
                        Permissions = new(modPerms),
                    },
                    Intention = OverwriteIntention.Positive,
                };
            }

            overwrites[e.AuthorId] = new()
            {
                Overwrite = new()
                {
                    TargetId = new(e.AuthorId),
                    TargetType = new(OverwriteTargetType.Member),
                    Permissions = new(ownerPerms),
                },
                Intention = OverwriteIntention.Positive,
            };

            Action.ChannelPermissionOverwrites = [..overwrites.Values];

            StringBuilder filteredBuilder = new();
            foreach (char character in e.Interaction.Author.Name.ToLowerInvariant())
            {
                if (AllowedChars.Contains(character))
                {
                    filteredBuilder.Append(character);
                }
            }
            Action.ChannelName = $"{filteredBuilder}s-channel";

            if (ReturnTo is not null)
            {
                _ = Task.Run(async () => await ReturnTo());
            }
            return ValueTask.CompletedTask;
        }

#pragma warning restore CA1822

        private static Action<LocalMessageBase> BuildMessage(AbstractAnomalyAction action)
        {
            LocalEmbed embed = new()
            {
                Title = "Configure your channel's permissions",
                Description = "Select users who should be given special permissions. (Being an admin overrides being banned.)",
                Color = new(TierProperties.GetForTier(action.Tier).Color),
                Footer = new(new()
                {
                    Text = "Tip: You'll have permissions to name your channel once it's created."
                }),
            };

            return (message) =>
            {
                message.AddEmbed(embed);
            };
        }
    }
}
