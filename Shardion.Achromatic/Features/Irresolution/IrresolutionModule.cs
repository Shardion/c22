using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot.Commands.Application;
using Qmmands;
using Shardion.Achromatic.Configuration;

namespace Shardion.Achromatic.Features.Irresolution
{
    public class IrresolutionModule : DiscordApplicationGuildModuleBase
    {
        private readonly OptionsMultiplexer _opt;

        public IrresolutionModule(OptionsMultiplexer opt)
        {
            _opt = opt;
        }

        [SlashCommand("settings")]
        [Description("View and change settings.")]
        public ValueTask<IResult> Settings(bool server)
        {
            OptionsAccessibility acc = server ? OptionsAccessibility.Servers : OptionsAccessibility.Users;
            IReadOnlyCollection<IBindableOptions> opts = _opt.GetMany<IBindableOptions>(acc, Context.AuthorId, Context.GuildId);

            return ValueTask.FromResult<IResult>(Menu(new IrresolutionSettingsMenu(opts, new IrresolutionSettingsView(opts.First()), Context.Interaction)));
        }
    }
}
