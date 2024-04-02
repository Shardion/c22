using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;

namespace Shardion.Achromatic.Extensions
{
    public static class DiscordBotBaseExtensions
    {
        public static Task<IUser?> GetOrFetchUser(this DiscordBotBase bot, Snowflake userId)
        {
            if (bot.GetUser(userId) is IUser cacheUser)
            {
                return Task.FromResult<IUser?>(cacheUser);
            }
            return bot.FetchUserAsync(userId).ContinueWith((task) =>
            {
                return task.Result as IUser;
            });
        }

        public static Task<IMember?> GetOrFetchMember(this DiscordBotBase bot, Snowflake guildId, Snowflake userId)
        {
            if (bot.GetMember(guildId, userId) is IMember cacheMember)
            {
                return Task.FromResult<IMember?>(cacheMember);
            }
            return bot.FetchMemberAsync(guildId, userId);
        }

        public static Task<IChannel?> GetOrFetchChannel(this DiscordBotBase bot, Snowflake guildId, Snowflake channelId)
        {
            if (bot.GetChannel(guildId, channelId) is IChannel cacheCh)
            {
                return Task.FromResult<IChannel?>(cacheCh);
            }
            return bot.FetchChannelAsync(channelId);
        }

        public static Task<IMessage?> GetOrFetchMessage(this DiscordBotBase bot, Snowflake channelId, Snowflake messageId)
        {
            if (bot.GetMessage(channelId, messageId) is IMessage message)
            {
                return Task.FromResult<IMessage?>(message);
            }
            return bot.FetchMessageAsync(channelId, messageId);
        }

        public static Task<IGuild?> GetOrFetchGuild(this DiscordBotBase bot, Snowflake guildId)
        {
            if (bot.GetGuild(guildId) is IGuild cacheGuild)
            {
                return Task.FromResult<IGuild?>(cacheGuild);
            }
            return bot.FetchGuildAsync(guildId);
        }
    }
}
