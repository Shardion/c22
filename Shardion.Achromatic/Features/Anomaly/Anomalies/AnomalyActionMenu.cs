using System.Collections.Generic;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands.Application;
using Disqord.Extensions.Interactivity.Menus;
using Qommon;
using Shardion.Achromatic.Common.Menus;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies
{
    public class AnomalyActionMenu : LoudRestrictedInteractionMenu
    {
        public IReadOnlyCollection<AbstractAnomalyAction> Actions { get; }
        public DiscordBotBase Bot { get; }
        public IDiscordApplicationGuildCommandContext Context { get; }

    public AnomalyActionMenu(IReadOnlyCollection<AbstractAnomalyAction> actions, ViewBase view, IUserInteraction interaction, DiscordBotBase bot, IDiscordApplicationGuildCommandContext context) : base(view, interaction)
        {
            Actions = actions;
            Bot = bot;
            Context = context;

            AddComponentsToView(actions, view, interaction);
        }

        private void AddComponentsToView(IReadOnlyCollection<AbstractAnomalyAction> actions, ViewBase view, IUserInteraction interaction)
        {
            Dictionary<string, AbstractAnomalyAction> namedActions = [];
            List<LocalSelectionComponentOption> selectionCategories = [];
            foreach (AbstractAnomalyAction action in actions)
            {
                namedActions.Add(action.Name, action);
                selectionCategories.Add(new()
                {
                    Label = action.Name,
                    Value = action.Name,
                });
            }

            view.AddComponent(new SelectionViewComponent(async (a) =>
            {
                if (a.SelectedOptions[0].Value.GetValueOrDefault() is not string actionName)
                {
                    return;
                }
                AnomalyActionView view = new(namedActions[actionName], Bot, Context);
                AddComponentsToView(actions, view, interaction);
                await SetViewAsync(view);
            })
            {
                Type = SelectionComponentType.String,
                Options = selectionCategories,
                Placeholder = "Select an action",
            });
        }
    }
}
