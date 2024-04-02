using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Qommon;
using Shardion.Achromatic.Common.Menus;
using Shardion.Achromatic.Configuration;

namespace Shardion.Achromatic.Features.Irresolution
{
    public class IrresolutionSettingsMenu : LoudRestrictedInteractionMenu
    {
        private readonly IReadOnlyCollection<IBindableOptions> _options;
        private readonly IrresolutionSettingsView OptionsView;

        public IrresolutionSettingsMenu(IReadOnlyCollection<IBindableOptions> options, IrresolutionSettingsView view, IUserInteraction interaction) : base(view, interaction)
        {
            _options = options;
            OptionsView = view;

            AddComponentsToView(view);
        }

        private IBindableOptions? GetNamedOptions(string? name)
        {
            return _options.First((x) => x.GetSectionName() == name);
        }

        private void AddComponentsToView(ViewBase view)
        {
            List<LocalSelectionComponentOption> selectionCategories = [];

            foreach (IBindableOptions bindable in _options)
            {
                selectionCategories.Add(new()
                {
                    Label = bindable.GetSectionName(),
                    Value = bindable.GetSectionName(),
                });
            }

            view.AddComponent(new SelectionViewComponent(async (a) =>
            {
                IBindableOptions? category = GetNamedOptions(a.SelectedOptions[0].Value.GetValueOrDefault());
                if (category is not null)
                {
                    IrresolutionSettingsView view = new(category);
                    AddComponentsToView(view);
                    await SetViewAsync(view);
                }
            })
            {
                Type = SelectionComponentType.String,
                Options = selectionCategories,
                Placeholder = "Select a category to view",
            });

            List<LocalSelectionComponentOption> selectionOptions = [];

            foreach (PropertyInfo prop in OptionsView.Options.GetType().GetProperties())
            {
                selectionOptions.Add(new()
                {
                    Label = prop.Name,
                    Value = prop.Name,
                });
            }

            view.AddComponent(new SelectionViewComponent((a) =>
            {
                return ValueTask.CompletedTask;
            })
            {
                Type = SelectionComponentType.String,
                Options = selectionOptions,
                Placeholder = "Select an option to change",
            });
        }
    }
}
