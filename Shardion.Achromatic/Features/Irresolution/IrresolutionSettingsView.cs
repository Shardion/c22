using System;
using System.Reflection;
using System.Text;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Shardion.Achromatic.Common;
using Shardion.Achromatic.Configuration;

namespace Shardion.Achromatic.Features.Irresolution
{
    public class IrresolutionSettingsView : ViewBase
    {
        public IBindableOptions Options { get; }

        public IrresolutionSettingsView(IBindableOptions options) : base(BuildEmbed(options))
        {
            Options = options;
        }

        private static Action<LocalMessageBase> BuildEmbed(IBindableOptions options)
        {
            StringBuilder builder = new();
            foreach (PropertyInfo property in options.GetType().GetProperties())
            {
                builder.AppendLine($"- **{property.Name}**: {FormattingHelper.FormatValue(property.GetValue(options), true)}");
            }

            return (message) =>
            {
                message.AddEmbed(new LocalEmbed()
                {
                    Title = "Settings",
                    Fields = new([
                    new()
                    {
                        Name = options.GetSectionName(),
                        Value = builder.ToString(),
                    }
                ]),
                });
            };
        }
    }
}
