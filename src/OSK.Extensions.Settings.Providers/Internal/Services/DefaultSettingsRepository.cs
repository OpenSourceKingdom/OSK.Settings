using OSK.Extensions.Settings.Providers.Ports;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Settings.Abstractions;
using OSK.Settings.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Extensions.Settings.Providers.Internal.Services
{
    internal class DefaultSettingsRepository(IEnumerable<ISettingsProvider> settingProviders,
        IOutputFactory<DefaultSettingsRepository> outputFactory) : ISettingsRepository
    {
        #region Variables

        private bool _hasInitialized;
        private readonly IDictionary<long, Setting> _settingLookup = new Dictionary<long, Setting>();
        private readonly IDictionary<string, IList<Setting>> _settingsByCategory = new Dictionary<string, IList<Setting>>();

        #endregion

        #region ISettingsRepository

        public async Task<IOutput<Setting>> GetAsync(long id, CancellationToken cancellationToken = default)
        {
            if (!_hasInitialized)
            {
                var initializeResult = await InitializeAsync(cancellationToken);
                if (!initializeResult.IsSuccessful)
                {
                    return initializeResult.AsType<Setting>();
                }
            }

            return _settingLookup.TryGetValue(id, out var setting)
                ? outputFactory.Success(setting)
                : outputFactory.NotFound<Setting>($"Setting with id {id} not found.");
        }

        public async Task<IOutput<IEnumerable<Setting>>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (!_hasInitialized)
            {
                var initializeResult = await InitializeAsync(cancellationToken);
                if (!initializeResult.IsSuccessful)
                {
                    return initializeResult.AsType<IEnumerable<Setting>>();
                }
            }

            var setting = _settingLookup.Values.FirstOrDefault(setting => string.Equals(setting.Name, name, StringComparison.Ordinal));
            return setting == null
                ? outputFactory.Success(Enumerable.Empty<Setting>())
                : outputFactory.Success((IEnumerable<Setting>)[setting]);
        }

        public async Task<IOutput<PaginatedOutput<Setting>>> GetPageAsync(SettingCategory? settingCategory, long skip, long take, CancellationToken cancellationToken = default)
        {
            if (!_hasInitialized)
            {
                var initializeResult = await InitializeAsync(cancellationToken);
                if (!initializeResult.IsSuccessful)
                {
                    return initializeResult.AsType<PaginatedOutput<Setting>>();
                }
            }

            var settings = Enumerable.Empty<Setting>();
            if (settingCategory is null)
            {
                settings = _settingLookup.Values;
            }
            else if (_settingsByCategory.TryGetValue(settingCategory.Value.Name, out var categoryList))
            {
                settings = categoryList;
            }

            return outputFactory.Success(new PaginatedOutput<Setting>()
            {
                Items = settings.Skip((int)skip).Take((int)take).ToList(),
                Skip = skip,
                Take = take,
                Total = _settingLookup.Count
            });
        }

        public Task<IOutput<Setting>> CreateAsync(Setting setting, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IOutput> DeleteAsync(long id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IOutput<Setting>> UpdateAsync(Setting setting, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helpers

        internal async ValueTask<IOutput> InitializeAsync(CancellationToken cancellationToken)
        {
            _hasInitialized = false;
            _settingsByCategory.Clear();
            _settingLookup.Clear();

            foreach (var provider in settingProviders)
            {
                var getSettingsResult = await provider.GetSettingsAsync(cancellationToken);
                if (!getSettingsResult.IsSuccessful)
                {
                    return getSettingsResult.AsType<List<Setting>>();
                }

                foreach (var setting in getSettingsResult.Value)
                {
                    _settingLookup[setting.Id] = setting;
                }
            }

            foreach (var settingGroup in _settingLookup.Values.GroupBy(setting => setting.Category?.Name ?? "_default"))
            {
                _settingsByCategory[settingGroup.Key] = settingGroup.ToList();
            } 

            _hasInitialized = true;
            return outputFactory.Success();
        }

        #endregion
    }
}
