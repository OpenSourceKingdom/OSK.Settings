using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OSK.Extensions.Settings.Providers.Internal.Services;
using OSK.Extensions.Settings.Providers.Ports;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Settings.Abstractions;
using OSK.Settings.Ports;
using System.Collections.Generic;
using System.Linq;

namespace OSK.Extensions.Settings.Provider
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSettingProviders(this IServiceCollection services)
        {
            services.TryAddTransient<ISettingsRepository, DefaultSettingsRepository>();
            services.TryAddTransient<ISettingValueRepository, DefaultSettingValueRepository>();

            return services;
        }

        public static IServiceCollection AddSettingsProvider<TProvider>(this IServiceCollection services)
            where TProvider : class, ISettingsProvider
        {
            services.AddSettingProviders();
            services.AddSingleton<ISettingsProvider, TProvider>();

            return services;
        }

        public static IServiceCollection AddSettingsProvider(this IServiceCollection services, IEnumerable<Setting> settings)
            => services.AddSettingsProvider(settings.ToArray());

        public static IServiceCollection AddSettingsProvider(this IServiceCollection services, params Setting[] settings)
        {
            services.AddSettingProviders();
            services.AddSingleton<ISettingsProvider>(servicerProvider 
                => new ConstantSettingsProvider(settings, servicerProvider.GetRequiredService<IOutputFactory<ConstantSettingsProvider>>()));

            return services;
        }

        public static IServiceCollection AddSettingValueProvider<TProvider>(this IServiceCollection services)
            where TProvider : class, ISettingValueProvider
        {
            services.AddSettingProviders();
            services.AddTransient<ISettingValueProvider, TProvider>();

            return services;
        }
    }
}
