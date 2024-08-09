using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OSK.Settings.Management.Internal.Services;
using OSK.Settings.Management.Ports;

namespace OSK.Settings.Management
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSettingsManagement(this IServiceCollection services)
        {
            services.AddSettings();
            services.TryAddTransient<ISettingsManager, DefaultSettingsManager>();

            return services;
        }
    }
}
