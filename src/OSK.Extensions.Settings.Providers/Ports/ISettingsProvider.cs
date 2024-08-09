using OSK.Functions.Outputs.Abstractions;
using OSK.Settings.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Extensions.Settings.Providers.Ports
{
    public interface ISettingsProvider
    {
        ValueTask<IOutput<IEnumerable<Setting>>> GetSettingsAsync(CancellationToken cancellationToken = default);
    }
}
