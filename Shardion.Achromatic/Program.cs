using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.Configuration;
using Disqord.Gateway;
using Disqord.Bot.Hosting;
using Shardion.Achromatic.Extensions;
using Microsoft.Extensions.DependencyInjection;
using LiteDB;
using Shardion.Achromatic.Configuration;
using Disqord;
using Shardion.Achromatic.Features.Anomaly.Shop;
using Shardion.Achromatic.Features.Anomaly.Anomalies;
using System.Threading;
using Shardion.Achromatic.Common.Timers;
using Flurl.Util;

namespace Shardion.Achromatic
{
    public class Program
    {
        public static DateTime StartTime { get; private set; }

        private static async Task Main(string[] args)
        {
            StartTime = DateTime.UtcNow;

            using IHost host = new HostBuilder()
                .ConfigureAppConfiguration((context, configuration) =>
                {
                    configuration.AddCommandLine(args);
                    configuration.AddEnvironmentVariables("ACHROMATIC_");
                    configuration.AddJsonFile(ResolveConfigLocation("config"), optional: true);
                    configuration.AddJsonFile(ResolveConfigLocation("token"));
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddAllBindableOptions(context);
                    LiteDatabase db = new(context.Configuration["Database:ConnectionString"]);
                    BsonMapper.Global.RegisterType
                    (
                        serialize: (Snowflake snowflake) => new BsonValue(snowflake.RawValue.ToInvariantString()),
                        deserialize: (bson) => new Snowflake(ulong.Parse(bson.AsString))
                    );

                    services.AddSingleton(db);
                    services.AddSingleton<OptionsMultiplexer>();
                })
                .UseSerilog((context, logger) =>
                {
                    logger.WriteTo.Console();
#if DEBUG
                    logger.MinimumLevel.Verbose();
#endif
                })
                .ConfigureDiscordBot((context, bot) =>
                {
                    bot.Token = context.Configuration["Token"]?.Trim();
                    bot.Intents = GatewayIntents.Unprivileged;
                    bot.UseMentionPrefix = false;
                })
                .Build();
            await host.RunAsync();
        }

        public static string ResolveConfigLocation(string filename, string extension = ".json")
        {
            foreach (string arg in Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith($"--{filename}="))
                {
                    return Path.Join(arg.Replace($"--{filename}=", ""), $"{filename}{extension}");
                }
            }
            if (Environment.GetEnvironmentVariable("ACHROMATIC_CONFIG_DIRECTORY") is string configDir)
            {
                return Path.Join(configDir, $"{filename}{extension}");
            }
            if (Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") is string configHome)
            {
                return Path.Join(configHome, "achromatic", $"{filename}{extension}");
            }
            if (Environment.GetEnvironmentVariable("HOME") is string home && Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                return Path.Join(home, ".config", "achromatic", $"{filename}{extension}");
            }
            return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "achromatic", $"{filename}{extension}");
        }
    }
}
