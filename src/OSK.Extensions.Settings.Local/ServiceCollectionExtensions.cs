using Microsoft.Extensions.DependencyInjection;
using OSK.Extensions.Settings.Providers.Ports;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Storage.Local;
using OSK.Storage.Local.Ports;
using System;

namespace OSK.Extensions.Settings.Local
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLocalSettingValueProvider(this IServiceCollection services, string filePath)
            => services.AddLocalSettingValueProvider(filePath, 0);

        public static IServiceCollection AddLocalSettingValueProvider(this IServiceCollection services, string filePath,
            int rank)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new InvalidOperationException($"File path cannot be empty.");
            }

            services.AddLocalStorage();
            services.AddTransient<ISettingValueProvider>(serviceProvider => new LocalStoragSettingValueProvider(filePath, rank,
                serviceProvider.GetRequiredService<ILocalStorageService>(),
                serviceProvider.GetRequiredService<IOutputFactory<LocalStoragSettingValueProvider>>()));

            return services;
        }
    }
}
