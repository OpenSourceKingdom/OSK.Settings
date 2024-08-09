using OSK.Functions.Outputs.Abstractions;
using OSK.Settings.Abstractions;
using OSK.Settings.Models;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Settings.Ports
{
    public interface ISettingsService
    {
        Task<IOutput<Setting>> CreateAsync(Setting setting, CancellationToken cancellationToken = default);

        Task<IOutput<Setting>> UpdateAsync(Setting setting, CancellationToken cancellationToken = default);

        Task<IOutput<PaginatedOutput<SettingValuePair>>> GetSettingValuePairsAsync(SettingCategory? settingCategory,
            long skip, long take, CancellationToken cancellationToken = default);

        Task<IOutput<EffectiveSetting<T>>> GetEffectiveSettingAsync<T>(long settingId, CancellationToken cancellationToken = default);
    }
}
