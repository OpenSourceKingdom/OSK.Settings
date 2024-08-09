using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OSK.Settings.Internal.Services;
using OSK.Settings.Ports;

namespace OSK.Settings
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSettings(this IServiceCollection services)
        {
            services.TryAddTransient<ISettingsService, SettingsService>();

            return services;
        }
    }
}
