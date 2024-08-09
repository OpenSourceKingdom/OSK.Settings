using OSK.Functions.Outputs.Abstractions;
using OSK.Settings.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Settings.Ports
{
    public interface ISettingValueRepository
    {
        Task<IOutput<SettingValue>> CreateAsync(SettingValue settingValue, CancellationToken cancellationToken = default);

        Task<IOutput<SettingValue>> UpdateAsync(SettingValue settingValue, CancellationToken cancellationToken = default);

        Task<IOutput<SettingValue>> GetAsync(long settingId, CancellationToken cancellationToken = default);

        Task<IOutput> DeleteAsync(long settigId, CancellationToken cancellationToken = default);

        Task<IOutput<IEnumerable<SettingValue>>> GetSettingValuesByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);

        Task<IOutput<IEnumerable<SettingValue>>> UpdateSettingValuesAsync(IEnumerable<SettingValue> settingValues, CancellationToken cancellationToken = default);
    }
}
