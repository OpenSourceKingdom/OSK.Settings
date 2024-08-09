using OSK.Functions.Outputs.Abstractions;
using OSK.Settings.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Extensions.Settings.Providers.Ports
{
    public interface ISettingValueProvider
    {
        /// <summary>
        /// The rank of the provider's values. This is used for considerations when multiple providers possess
        /// the same setting (i.e. cloud vs. local settings) where the provider with the lower rank will prevail.
        /// </summary>
        int Rank { get; }

        Task<IOutput<IEnumerable<SettingValue>>> GetSettingValuesAsync(CancellationToken cancellationToken = default);
    
        Task<IOutput<IEnumerable<SettingValue>>> UpsertValuesAsync(IEnumerable<SettingValue> settingValues, 
            CancellationToken cancellationToken = default);

        Task<IOutput> DeleteAsync(long settingId, CancellationToken cancellationToken = default);
    }
}
