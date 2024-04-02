using System;
using System.Text;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using LiteDB;
using Shardion.Achromatic.Common.Constants;
using Shardion.Achromatic.Configuration;

namespace Shardion.Achromatic.Features.Anomaly.Shop
{
    public class ShopRewardsView : ViewBase
    {
        public ShopRewardsView(AnomalyOptions options, AnomalyDatabaseService banker, IMember member) : base(BuildEmbed(options, banker, member))
        {
        }

        private static Action<LocalMessageBase> BuildEmbed(AnomalyOptions options, AnomalyDatabaseService banker, IMember member)
        {
            int ownedZebras = banker.GetZebras(member);

            StringBuilder meterBuilder = new();
            for (int segmentNumber = 0; segmentNumber < 20; segmentNumber++)
            {
                if (segmentNumber < ownedZebras)
                {
                    meterBuilder.Append(UnicodeEmojis.ZEBRA.Name);
                }
                else if (segmentNumber > 0 && (segmentNumber + 1) % 5 == 0 && (segmentNumber + 1) != 15)
                {
                    meterBuilder.Append(UnicodeEmojis.GIFT.Name);
                }
                else
                {
                    meterBuilder.Append(UnicodeEmojis.BLACK_LARGE_SQUARE.Name);
                }
            }

            LocalEmbed embed = new()
            {
                Title = "🦓️ Shop"
            };
            LocalEmbedField meterField = new()
            {
                Name = new("Rewards"),
                Value = new(meterBuilder.ToString()),
                IsInline = new(false),
            };
            LocalEmbedField explanationEarningField = new()
            {
                Name = new("How to Earn 🦓️s"),
                Value = new("""
                            🦓️s can be earned in two ways:
                            - Every time you help successfully stop an Anomaly from acting, you get 5 🦓️s!
                            - Every time you successfully perform an action as an Anomaly, you get 5 🦓️s!
                            """),
                IsInline = new(false),
            };
            LocalEmbedField explanationUsingField = new()
            {
                Name = new("How to Use 🦓️s"),
                Value = new("Rewards are claimed automatically and announced in <#712396995725230135>!"),
                IsInline = new(false),
            };
            embed.AddField(meterField);
            embed.AddField(explanationEarningField);
            embed.AddField(explanationUsingField);
            return (message) => message.AddEmbed(embed);
        }
    }
}
