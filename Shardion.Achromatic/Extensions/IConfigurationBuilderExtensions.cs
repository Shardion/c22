using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Shardion.Achromatic.Configuration;
using Shardion.Achromatic.Common;

namespace Shardion.Achromatic.Extensions
{
    public static class IHostBuilderExtensions
    {
        public static IServiceCollection AddAllBindableOptions(this IServiceCollection services, HostBuilderContext context)
        {
            foreach (IBindableOptions bindable in ReflectionHelper.ConstructParameterlessAssignables<IBindableOptions>())
            {
                context.Configuration.GetSection(bindable.GetSectionName()).Bind(bindable);
                services.AddSingleton(typeof(IBindableOptions), bindable);
                services.AddSingleton(bindable.GetType(), bindable);
            }
            return services;
        }
    }
}
