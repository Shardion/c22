using System;
using System.Collections.Generic;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Shardion.Achromatic.Common;

namespace Shardion.Achromatic.Configuration
{
    public class OptionsMultiplexer
    {
        private readonly LiteDatabase _db;
        private readonly IServiceProvider _provider;

        public OptionsMultiplexer(LiteDatabase db, IServiceProvider provider)
        {
            _db = db;
            _provider = provider;
        }

        public TOptions? Get<TOptions>(OptionsAccessibility accessibility, ulong? userId, ulong? serverId) where TOptions : class, IBindableOptions, new()
        {
            TOptions options = GetInternal<TOptions>(userId, serverId) ?? new();
            OptionsAccessibility requiredAccessibility = options.GetAccessibility();
            if (accessibility == requiredAccessibility || accessibility == OptionsAccessibility.Internal)
            {
                return options;
            }
            return null;
        }

        public IReadOnlyCollection<TOptions> GetMany<TOptions>(OptionsAccessibility accessibility, ulong? userId, ulong? serverId) where TOptions : IBindableOptions
        {
            List<TOptions> gotOptions = [];
            foreach (Type type in ReflectionHelper.GetParameterlessConstructibleAssignables<TOptions>())
            {
                if (GetInternal(type, userId, serverId) is TOptions options)
                {
                    OptionsAccessibility requiredAccessibility = options.GetAccessibility();
                    if (requiredAccessibility == accessibility || requiredAccessibility == OptionsAccessibility.Internal)
                    {
                        gotOptions.Add(options);
                    }
                }
            }
            return gotOptions.AsReadOnly();
        }

        private TOptions? GetInternal<TOptions>(ulong? userId, ulong? serverId) where TOptions : IBindableOptions, new()
        {
            if (userId is not null)
            {
                ILiteCollection<IdConfigPair> userConfigs = _db.GetCollection<IdConfigPair>("UserConfigurations");
                IdConfigPair? nullablePair = userConfigs.FindOne(x => x.Id == userId);
                if (nullablePair is IdConfigPair pair && pair.Configuration is TOptions conf)
                {
                    return conf;
                }
            }

            if (serverId is not null)
            {
                ILiteCollection<IdConfigPair> serverConfigs = _db.GetCollection<IdConfigPair>("ServerConfigurations");
                IdConfigPair? nullablePair = serverConfigs.FindOne(x => x.Id == serverId);
                if (nullablePair is IdConfigPair pair && pair.Configuration is TOptions conf)
                {
                    return conf;
                }
            }

            return _provider.GetService<TOptions>();
        }

        private IBindableOptions? GetInternal(Type type, ulong? userId, ulong? serverId)
        {
            if (userId is not null)
            {
                ILiteCollection<IdConfigPair> userConfigs = _db.GetCollection<IdConfigPair>("UserConfigurations");
                IdConfigPair? nullablePair = userConfigs.FindOne(x => x.Id == userId);
                if (nullablePair is IdConfigPair pair && type.IsAssignableFrom(pair.Configuration.GetType()))
                {
                    return pair.Configuration;
                }
            }

            if (serverId is not null)
            {
                ILiteCollection<IdConfigPair> serverConfigs = _db.GetCollection<IdConfigPair>("ServerConfigurations");
                IdConfigPair? nullablePair = serverConfigs.FindOne(x => x.Id == serverId);
                if (nullablePair is IdConfigPair pair && type.IsAssignableFrom(pair.Configuration.GetType()))
                {
                    return pair.Configuration;
                }
            }

            return _provider.GetService(type) as IBindableOptions;
        }
    }
}
