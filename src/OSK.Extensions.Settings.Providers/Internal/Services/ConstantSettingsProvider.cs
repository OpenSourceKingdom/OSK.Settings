using OSK.Extensions.Settings.Providers.Ports;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Settings.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Extensions.Settings.Providers.Internal.Services
{
    internal class ConstantSettingsProvider(IEnumerable<Setting> settings, IOutputFactory<ConstantSettingsProvider> outputFactory) : ISettingsProvider
    {
        public ValueTask<IOutput<IEnumerable<Setting>>> GetSettingsAsync(CancellationToken cancellationToken = default)
        {
            return new ValueTask<IOutput<IEnumerable<Setting>>>(outputFactory.Success(settings));
        }
    }
}
