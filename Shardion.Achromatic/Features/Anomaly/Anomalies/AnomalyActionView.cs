using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands.Application;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;
using LiteDB;
using Shardion.Achromatic.Features.Anomaly.Shop;

namespace Shardion.Achromatic.Features.Anomaly.Anomalies
{
    public class AnomalyActionView : ViewBase
    {
        public AbstractAnomalyAction Action { get; }
        public DiscordBotBase Bot { get; }
        public IDiscordApplicationGuildCommandContext Context { get; }

        public AnomalyActionView(AbstractAnomalyAction action, DiscordBotBase bot, IDiscordApplicationGuildCommandContext context) : base(BuildMessage(action))
        {
            Action = action;
            Bot = bot;
            Context = context;
        }

#pragma warning disable CA1822 // Has to be instance so disqord picks up on it

        [Button(Emoji = "💥", Label = "Use this Anomaly Action", Style = LocalButtonComponentStyle.Success)]
        public async ValueTask ClickMe(ButtonEventArgs e)
        {
            e.Button.IsDisabled = true;
            await e.Interaction.Response().DeferAsync();

            bool playerRichEnough = Action.Anomaly.Database.HasPlayPower(Action.Anomaly.Member, Action.Cost);
            if (!playerRichEnough)
            {
                Menu.View = new EmptyView((x) => x.WithContent("come back when you're... mmmm... RICHER").WithEmbeds([]));
                await Menu.ApplyChangesAsync();
                return;
            }

            Action.Anomaly.Manager.MessageActionStatus[Menu.MessageId] = new();

            if (Action.GetPreparationView(ContinueAction) is AbstractReturnableView preparationView)
            {
                Menu.View = preparationView;
                await Menu.ApplyChangesAsync();
            }
            else
            {
                await ContinueAction();
            }
        }

#pragma warning restore CA1822

        private async Task ContinueAction()
        {
            bool playerRichEnough = await Action.Anomaly.Database.SpendPlayPower(Action.Anomaly.Member, Action.Cost);
            if (!playerRichEnough)
            {
                Menu.View = new EmptyView((x) => x.WithContent("come back when you're... mmmm... RICHER").WithEmbeds([]));
                await Menu.ApplyChangesAsync();
                return;
            }

            ILiteCollection<AnomalyStatisticsRecord> records = Action.Anomaly.Database.Database.GetCollection<AnomalyStatisticsRecord>("anomalyStats");
            AnomalyStatisticsRecord record = AnomalyDatabaseService.FindOrCreateStatisticsRecord(records, Context.Author);
            record.ActionsUsed++;
            records.Update(record);

            if (Action.ChargeNeeded)
            {
                Task chargeTask = Action.Charge();
                Menu.View = Action.GetWarningView();
                await Menu.ApplyChangesAsync();
                await chargeTask;
            }

            if (Action.Anomaly.Manager.MessageActionStatus.Remove(Menu.MessageId, out ActionStatus? nullableStatus) && nullableStatus is ActionStatus status)
            {
                if (status.Status == ActionCompletionStatus.Failed)
                {
                    Menu.View = Action.GetFailureView();
                    await Menu.ApplyChangesAsync();

                    record = AnomalyDatabaseService.FindOrCreateStatisticsRecord(records, Context.Author);
                    record.ActionsUsedCancelled++;
                    records.Update(record);

                    foreach (IMember member in status.Users)
                    {
                        AnomalyStatisticsRecord memberRecord = AnomalyDatabaseService.FindOrCreateStatisticsRecord(records, member);
                        memberRecord.ActionsStopped++;
                        records.Update(memberRecord);
                        await Action.Anomaly.Database.AddZebras(member, 5, true);
                    }
                }
                else
                {
                    Task completeTask = Action.Complete();
                    Menu.View = Action.GetCompletionView();
                    await Menu.ApplyChangesAsync();
                    await completeTask;
                    record = AnomalyDatabaseService.FindOrCreateStatisticsRecord(records, Context.Author);
                    record.ActionsUsedSuccessful++;
                    records.Update(record);
                    await Action.Anomaly.Database.AddZebras(Context.Author, 5, true);
                }
            }
        }


        private static Action<LocalMessageBase> BuildMessage(AbstractAnomalyAction action)
        {
            LocalEmbed embed = new()
            {
                Title = action.Name,
                Description = action.Description,
                Color = new(TierProperties.GetForTier(action.Tier).Color),
            };
            LocalEmbedField tierField = new()
            {
                Name = "Tier",
                Value = new(TierProperties.GetForTier(action.Tier).Name),
                IsInline = true,
            };
            LocalEmbedField costField = new()
            {
                Name = "Cost",
                Value = new($"{action.Cost}<:pp:1218384877951516673>"),
                IsInline = true,
            };
            LocalEmbedField chargeField = new()
            {
                Name = "Charge Time",
                Value = new($"{action.ChargeSeconds}s"),
                IsInline = true,
            };
            embed.AddField(tierField);
            embed.AddField(costField);
            embed.AddField(chargeField);

            return (message) =>
            {
                message.AddEmbed(embed);
            };
        }
    }
}
