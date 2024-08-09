using OSK.Functions.Outputs.Abstractions;
using OSK.Settings.Abstractions;
using OSK.Settings.Management.Models;
using OSK.Settings.Models;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Settings.Management.Ports
{
    public interface ISettingsManager
    {
        IOutput StageSettingUpdate(ManagedSetting managedSetting, object value);

        void ResetStagedUpdates();

        Task<IOutput<EffectiveSetting<T>>> GetEffectiveSettingAsync<T>(long settingId, CancellationToken cancellationToken = default);

        Task<IOutput<PaginatedOutput<ManagedSetting>>> GetSettingsByPageAsync(SettingCategory? category, long skip, long take,
            CancellationToken cancellationToken = default);

        Task<IOutput> ApplySettingsAsync(CancellationToken cancellationToken = default);
    }
}
