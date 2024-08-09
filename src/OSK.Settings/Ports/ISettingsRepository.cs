using OSK.Functions.Outputs.Abstractions;
using OSK.Settings.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Settings.Ports
{
    public interface ISettingsRepository
    {
        Task<IOutput<Setting>> CreateAsync(Setting setting, CancellationToken cancellationToken = default);

        Task<IOutput<Setting>> UpdateAsync(Setting setting, CancellationToken cancellationToken = default);

        Task<IOutput<Setting>> GetAsync(long id, CancellationToken cancellationToken = default);

        Task<IOutput> DeleteAsync(long id, CancellationToken cancellationToken = default);

        Task<IOutput<IEnumerable<Setting>>> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<IOutput<PaginatedOutput<Setting>>> GetPageAsync(SettingCategory? settingCategory,
            long skip, long take, CancellationToken cancellationToken = default);
    }
}
