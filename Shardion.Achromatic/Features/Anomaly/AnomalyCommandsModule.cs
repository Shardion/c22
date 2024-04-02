using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands.Application;
using Qmmands;
using LiteDB;
using Shardion.Achromatic.Features.Anomaly.Shop;
using Shardion.Achromatic.Features.Anomaly.Anomalies;
using Shardion.Achromatic.Common.Menus;
using Shardion.Achromatic.Configuration;
using System.Linq;
using System;
using Disqord.Rest;
using System.Collections.Generic;

namespace Shardion.Achromatic.Features.Anomaly
{
    [SlashGroup("anomaly")]
    public class AnomalyCommandsModule : DiscordApplicationGuildModuleBase
    {
        private readonly OptionsMultiplexer _opt;
        private readonly AnomalyOptions _anomalyOpt;
        private readonly AnomalyDatabaseService _database;
        private readonly AnomalyManagerService _manager;

        public AnomalyCommandsModule(OptionsMultiplexer options, AnomalyDatabaseService banker, AnomalyManagerService manager)
        {
            _opt = options;
            _anomalyOpt = options.Get<AnomalyOptions>(OptionsAccessibility.NoOne, null, null) ?? new();
            _database = banker;
            _manager = manager;
        }

        [SlashCommand("shop")]
        [Description("View the Zebra Shop.")]
        public ValueTask<IResult> Shop()
        {
            return ValueTask.FromResult<IResult>(Menu(new LoudRestrictedInteractionMenu(new ShopRewardsView(_anomalyOpt, _database, Context.Author), Context.Interaction)));
        }

        [SlashCommand("action")]
        [Description("View the Anomaly Menu.")]
        public ValueTask<IResult> Action()
        {
            ILiteCollection<AnomalyRecord> records = _database.Database.GetCollection<AnomalyRecord>("anomaly");
            AnomalyRecord record = AnomalyDatabaseService.FindOrCreateRecord(records, Context.Author);
            if (_manager.GetAnomaly(Context, Context.Author, record) is not AbstractAnomaly anomaly)
            {
                return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "You aren't an Anomaly!",
                }));
            }
            return ValueTask.FromResult<IResult>(Menu(new AnomalyActionMenu(anomaly.GetActions(), new AnomalyOverviewView(anomaly.Record), Context.Interaction, Bot, Context)
            {
                AllowedUsers = new HashSet<Snowflake>() { Context.Interaction.Author.Id },
            }));
        }

        [SlashCommand("info")]
        [Description("Displays info about your type of Anomaly.")]
        public ValueTask<IResult> Info()
        {
            ILiteCollection<AnomalyRecord> records = _database.Database.GetCollection<AnomalyRecord>("anomaly");
            AnomalyRecord record = AnomalyDatabaseService.FindOrCreateRecord(records, Context.Author);
            if (_manager.GetAnomaly(Context, Context.Author, record) is not AbstractAnomaly anomaly)
            {
                return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "You aren't an Anomaly!",
                }));
            }

            return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
            {
                IsEphemeral = true,
                Content = $"You are **\"{anomaly.Name}\"**.",
            }));
        }

        [SlashCommand("debug")]
        [Description("Developer only. Force-generates an Anomaly Record.")]
        public ValueTask<IResult> Debug(IMember member)
        {
            if (!(_opt.Get<IdentityOptions>(OptionsAccessibility.NoOne, null, null) ?? new()).DeveloperIDs.Contains(Context.AuthorId))
            {
                return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
                {
                    Content = LoudRestrictedMenuBase.Responses[Random.Shared.Next(LoudRestrictedMenuBase.Responses.Length)],
                }));
            }

            ILiteCollection<AnomalyRecord> anomalyRecords = _database.Database.GetCollection<AnomalyRecord>("anomaly");
            AnomalyRecord memberRecord = AnomalyDatabaseService.FindOrCreateRecord(anomalyRecords, member);

            return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
            {
                IsEphemeral = true,
                Content = $"{member.Mention} has an Anomaly Record as {memberRecord.Anomaly} with {memberRecord.Zebras} Zebras and {memberRecord.PlayPower}/6 pp.",
            }));
        }

        [SlashCommand("fuse")]
        [Description("Join forces with other Anomalies.")]
        public ValueTask<IResult> Fuse(IMember member)
        {
            ILiteCollection<AnomalyRecord> records = _database.Database.GetCollection<AnomalyRecord>("anomaly");
            AnomalyRecord startingRecord = AnomalyDatabaseService.FindOrCreateRecord(records, Context.Author);
            AnomalyRecord otherRecord = AnomalyDatabaseService.FindOrCreateRecord(records, member);

            if (Context.AuthorId == member.Id)
            {
                return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "You can't fuse with yourself!",
                }));
            }

            if (_manager.GetAnomaly(Context, Context.Author, startingRecord) is not AbstractAnomaly startingAnomaly)
            {
                return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "You aren't an Anomaly!",
                }));
            }
            if (_manager.GetAnomaly(Context, member, otherRecord) is not AbstractAnomaly otherAnomaly)
            {
                return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "The other member isn't an Anomaly!",
                }));
            }
            if (startingAnomaly.Record.Tier == AnomalyTier.Joined)
            {
                return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "You've already joined two Anomalies (check out `/anomaly fuse-three`)!",
                }));
            }
            if (otherAnomaly.Record.Tier == AnomalyTier.Joined)
            {
                return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "The Anomaly you're trying to fuse with has already joined two Anomalies (check out `/anomaly fuse-three`)!",
                }));
            }

            if (startingAnomaly.Record.Anomaly == otherAnomaly.Record.Anomaly)
            {
                return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "Identical Anomalies can't join together!",
                }));
            }

            return ValueTask.FromResult<IResult>(Menu(new LoudRestrictedInteractionMenu(new JoinTogetherTwoAnomaliesView(startingAnomaly, otherAnomaly), Context.Interaction)));
        }

        [SlashCommand("fuse-three")]
        [Description("Join forces with other Anomalies.")]
        public ValueTask<IResult> FuseThree(IMember secondMember, IMember thirdMember)
        {
            ILiteCollection<AnomalyRecord> records = _database.Database.GetCollection<AnomalyRecord>("anomaly");
            AnomalyRecord startingRecord = AnomalyDatabaseService.FindOrCreateRecord(records, Context.Author);
            AnomalyRecord secondRecord = AnomalyDatabaseService.FindOrCreateRecord(records, secondMember);
            AnomalyRecord thirdRecord = AnomalyDatabaseService.FindOrCreateRecord(records, thirdMember);

            bool firstSecondMembersIdentical = Context.AuthorId == secondMember.Id;
            bool secondThirdMembersIdentical = secondMember.Id == thirdMember.Id;
            bool thirdFirstMembersIdentical = thirdMember.Id == Context.AuthorId;
            if (firstSecondMembersIdentical || secondThirdMembersIdentical || thirdFirstMembersIdentical)
            {
                return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "You can't fuse with yourself!",
                }));
            }

            if (_manager.GetAnomaly(Context, Context.Author, startingRecord) is not AbstractAnomaly startingAnomaly)
            {
                return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "You aren't an Anomaly!",
                }));
            }
            if (_manager.GetAnomaly(Context, secondMember, secondRecord) is not AbstractAnomaly secondAnomaly)
            {
                return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "The second member isn't an Anomaly!",
                }));
            }
            if (_manager.GetAnomaly(Context, thirdMember, thirdRecord) is not AbstractAnomaly thirdAnomaly)
            {
                return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "The third member isn't an Anomaly!",
                }));
            }

            if (startingAnomaly.Record.Tier == AnomalyTier.RealityThreatening)
            {
                return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "You can't fuse any further!",
                }));
            }
            if (secondAnomaly.Record.Tier == AnomalyTier.RealityThreatening)
            {
                return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "The second member can't fuse any further!",
                }));
            }
            if (thirdAnomaly.Record.Tier == AnomalyTier.RealityThreatening)
            {
                return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "The third member can't fuse any further!",
                }));
            }

            bool firstSecondIdentical = startingAnomaly.Record.Anomaly == secondAnomaly.Record.Anomaly;
            bool secondThirdIdentical = secondAnomaly.Record.Anomaly == thirdAnomaly.Record.Anomaly;
            bool thirdFirstIdentical = thirdAnomaly.Record.Anomaly == startingAnomaly.Record.Anomaly;
            if (firstSecondIdentical || secondThirdIdentical || thirdFirstIdentical)
            {
                return ValueTask.FromResult<IResult>(Response(new LocalInteractionMessageResponse()
                {
                    IsEphemeral = true,
                    Content = "Identical Anomalies can't join together!",
                }));
            }

            return ValueTask.FromResult<IResult>(Menu(new LoudRestrictedInteractionMenu(new JoinTogetherThreeAnomaliesView(startingAnomaly, secondAnomaly, thirdAnomaly), Context.Interaction)));
        }

        [SlashCommand("paint")]
        [Description("Edit a role that you've previously painted into existence.")]
        public async ValueTask<IResult> Paint(string roleName, int red, int green, int blue)
        {
            ILiteCollection<AnomalyRecord> records = _database.Database.GetCollection<AnomalyRecord>("anomaly");
            AnomalyRecord record = AnomalyDatabaseService.FindOrCreateRecord(records, Context.Author);

            await Bot.ModifyRoleAsync(Context.GuildId, record.MostRecentPaintedRole, (e) =>
            {
                e.Name = roleName;
                e.Color = new(new((byte)red, (byte)green, (byte)blue));
            });
            return Response(new LocalInteractionMessageResponse()
            {
                Content = "Your most recently-created role has been updated.",
                IsEphemeral = true,
            });
        }
    }
}
