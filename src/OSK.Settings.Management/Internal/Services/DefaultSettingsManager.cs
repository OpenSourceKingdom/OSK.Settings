using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Settings.Abstractions;
using OSK.Settings.Abstractions.Validation;
using OSK.Settings.Management.Models;
using OSK.Settings.Management.Ports;
using OSK.Settings.Models;
using OSK.Settings.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Settings.Management.Internal.Services
{
    internal class DefaultSettingsManager(ISettingsService settingsService, ISettingValueRepository settingValueRepository, 
        IOutputFactory<DefaultSettingsManager> outputFactory) : ISettingsManager
    {
        #region Variables

        internal Dictionary<long, ManagedSetting> _stagedUpdates { get; set; } = new();

        #endregion

        #region ISettingsManager

        public async Task<IOutput> ApplySettingsAsync(CancellationToken cancellationToken = default)
        {
            if (_stagedUpdates.Count == 0)
            {
                return outputFactory.Success();
            }

            var settingValues = _stagedUpdates.Values.Select(managedSetting => managedSetting.GetSettingValue());
            var updateResult = await settingValueRepository.UpdateSettingValuesAsync(settingValues, cancellationToken);
            if (updateResult.IsSuccessful)
            {
                ResetStagedUpdates();
            }
            return updateResult;
        }

        public void ResetStagedUpdates()
        {
            foreach (var managedSetting in _stagedUpdates)
            {
                managedSetting.Value.HasSetValue = false;
            }

            _stagedUpdates.Clear();
        }

        public Task<IOutput<EffectiveSetting<T>>> GetEffectiveSettingAsync<T>(long id, CancellationToken cancellationToken = default)
        {
            return settingsService.GetEffectiveSettingAsync<T>(id, cancellationToken);
        }

        public async Task<IOutput<PaginatedOutput<ManagedSetting>>> GetSettingsByPageAsync(SettingCategory? category, long skip, long take, CancellationToken cancellationToken = default)
        {
            var getSettingValuePairsResult = await settingsService.GetSettingValuePairsAsync(category, skip, take, cancellationToken);
            if (!getSettingValuePairsResult.IsSuccessful)
            {
                return getSettingValuePairsResult.AsType<PaginatedOutput<ManagedSetting>>();
            }

            return outputFactory.Success(new PaginatedOutput<ManagedSetting>()
            {
                Skip = skip,
                Take = take,
                Total = getSettingValuePairsResult.Value.Total,
                Items = getSettingValuePairsResult.Value.Items.Select(item => new ManagedSetting(item, this)).ToList()
            });
        }

        public IOutput StageSettingUpdate(ManagedSetting managedSetting, object value)
        {
            if (managedSetting == null)
            {
                throw new ArgumentNullException(nameof(managedSetting));
            }

            var setting = managedSetting.GetSetting();
            var validationResult = setting.ValidateValue(value);
            if (validationResult.ResultType != ValidationResultType.Valid)
            {
                return outputFactory.BadRequest($"Unable to stage the update for the setting due to an error.\n{string.Join(",", validationResult.Errors)}");
            }

            managedSetting.HasSetValue = true;
            _stagedUpdates.Add(setting.Id, managedSetting);
            return outputFactory.Success();
        }

        #endregion
    }
}
