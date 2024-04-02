using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands.Application;
using Disqord.Rest;
using Qmmands;
using Shardion.Achromatic.Common;
using Shardion.Achromatic.Configuration;

namespace Shardion.Achromatic.Features.About
{
    public class AboutModule(IdentityOptions id) : DiscordApplicationModuleBase
    {
        [SlashCommand("stats")]
        [Description("Displays numerous interesting stats relevant to the bot.")]
        public ValueTask<IResult> Stats()
        {
            IReadOnlyCollection<IStatsEntry> stats = ReflectionHelper.ConstructParameterlessAssignables<IStatsEntry>();
            List<LocalEmbedField> statFields = [];
            foreach (IStatsEntry stat in stats)
            {
                statFields.Add(new()
                {
                    Name = stat.Name,
                    Value = stat.GetStatistic(),
                    IsInline = true,
                });
            }
            return ValueTask.FromResult<IResult>(Response(new LocalEmbed()
            {
                Title = new("Stats"),
                Timestamp = new(DateTimeOffset.UtcNow),
                Color = new(new(id.Color)),
                Fields = new(statFields),
            }));
        }

        [SlashCommand("about")]
        [Description("Displays anecdotal info relevant to the bot.")]
        public async ValueTask<IResult> About()
        {
            IInvite invite = await Bot.CreateInviteAsync(741037240276484116, maxAge: new(0, 15, 0), maxUses: 1, options: new DefaultRestRequestOptions
            {
                Reason = "To be displayed in /about"
            });

            return Response(new LocalEmbed()
            {
                Title = new("About C22"),
                Timestamp = new(DateTimeOffset.UtcNow),
                Color = new(new(31, 30, 51)),
                Fields = new(new LocalEmbedField[]
                    {
                        new()
                        {
                            Name = "Who made it?",
                            Value = $"The C22 project is being developed by shardion to fit the common requirements\nof the Ench Table server. (All are [invited](<https://discord.gg/{invite.Code}>).)"
                        },
                        new()
                        {
                            Name = "What does \"C22\" mean?",
                            Value = "Whatever you want! It had a meaning at some point, but I removed it.",
                            IsInline = true,
                        }
                    })
            });
        }
    }
}
