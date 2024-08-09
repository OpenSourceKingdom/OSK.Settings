using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Settings.Abstractions;
using OSK.Settings.Abstractions.Validation;
using OSK.Settings.Models;
using OSK.Settings.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Settings.Internal.Services
{
    internal class SettingsService(ISettingsRepository settingsRepository, ISettingValueRepository valueRepository,
        IOutputFactory<SettingsService> outputFactory) : ISettingsService
    {
        #region ISettingsService

        public async Task<IOutput<Setting>> CreateAsync(Setting setting, CancellationToken cancellationToken = default)
        {
            if (setting == null)
            {
                throw new ArgumentNullException(nameof(setting));
            }

            var validationResult = await ValidateAsync(setting, cancellationToken);
            if (!validationResult.IsSuccessful)
            {
                return validationResult.AsType<Setting>();
            }

            return await settingsRepository.CreateAsync(setting, cancellationToken);
        }

        public async Task<IOutput<Setting>> UpdateAsync(Setting setting, CancellationToken cancellationToken = default)
        {
            if (setting == null)
            {
                throw new ArgumentNullException(nameof(setting));
            }

            var getSettingResult = await settingsRepository.GetAsync(setting.Id, cancellationToken);
            if (!getSettingResult.IsSuccessful)
            {
                return getSettingResult;
            }

            var validationResult = await ValidateAsync(setting, cancellationToken);
            if (!validationResult.IsSuccessful)
            {
                return validationResult.AsType<Setting>();
            }

            return await settingsRepository.UpdateAsync(setting, cancellationToken);
        }

        public async Task<IOutput<EffectiveSetting<T>>> GetEffectiveSettingAsync<T>(long settingId, CancellationToken cancellationToken = default)
        {
            var getSettingResult = await settingsRepository.GetAsync(settingId, cancellationToken);
            if (!getSettingResult.IsSuccessful)
            {
                return getSettingResult.AsType<EffectiveSetting<T>>();
            }
            if (getSettingResult.Value is not Setting<T> setting)
            {
                return outputFactory.BadRequest<EffectiveSetting<T>>($"The setting id, {settingId}, refers to a setting of type {getSettingResult.Value.GetType().FullName}, which does not support types of {typeof(T).FullName}.", Constants.OriginSource);
            }

            var getSettingValueResult = await valueRepository.GetAsync(settingId);
            if (!getSettingValueResult.IsSuccessful)
            {
                return
                    getSettingValueResult.Code.StatusCode == HttpStatusCode.NotFound
                    ? outputFactory.Success(new EffectiveSetting<T>(setting, setting.DefaultValue))
                    : getSettingValueResult.AsType<EffectiveSetting<T>>();
            }
            if (getSettingValueResult.Value is not SettingValue<T> settingValue)
            {
                return outputFactory.BadRequest<EffectiveSetting<T>>($"The setting value, {getSettingValueResult.Value.GetType().FullName}, is not of the expected setting value type, {typeof(T).FullName}.", Constants.OriginSource);
            }

            return outputFactory.Success(new EffectiveSetting<T>(setting, settingValue.Value));
        }

        public async Task<IOutput<PaginatedOutput<SettingValuePair>>> GetSettingValuePairsAsync(SettingCategory? settingCategory,
            long skip, long take, CancellationToken cancellationToken = default)
        {
            var getPageResult = await settingsRepository.GetPageAsync(settingCategory, skip, take, cancellationToken);
            if (!getPageResult.IsSuccessful)
            {
                return getPageResult.AsType<PaginatedOutput<SettingValuePair>>();
            }

            var getSettingValuesResult = await valueRepository.GetSettingValuesByIdsAsync(getPageResult.Value.Items.Select(item => item.Id), cancellationToken);
            if (!getSettingValuesResult.IsSuccessful)
            {
                return getSettingValuesResult.AsType<PaginatedOutput<SettingValuePair>>();
            }

            var settingValueLookup = getSettingValuesResult.Value.ToDictionary(settingValue => settingValue.SettingId);
            var settingValuePairs = new List<SettingValuePair>();
            foreach (var setting in getPageResult.Value.Items)
            {
                var settingValuePair = settingValueLookup.TryGetValue(setting.Id, out var settingValue)
                    ? settingValue.ToSettingValuePair(setting)
                    : setting.GetDefaultSettingValuePair();

                settingValuePairs.Add(settingValuePair);
            }

            return outputFactory.Success(new PaginatedOutput<SettingValuePair>()
            {
                Total = getPageResult.Value.Total,
                Items = settingValuePairs,
                Skip = skip,
                Take = take
            });
        }

        #endregion

        #region Helpers

        private ValueTask<IOutput> ValidateAsync(Setting setting, CancellationToken cancellationToken)
        {
            var validationResult = setting.ValidateInternalParameters();
            if (validationResult.ResultType == ParameterValidationResultType.Invalid)
            {
                return new ValueTask<IOutput>(outputFactory.BadRequest($"Setting parameters are invalid.\n{string.Join("\n", validationResult.Errors)}"));
            }

            if (string.IsNullOrWhiteSpace(setting.Name))
            {
                return new ValueTask<IOutput>(outputFactory.BadRequest("Setting name can not be empty.", Constants.OriginSource));
            }

            return ValidateDuplicateNameAsync(setting, cancellationToken);
        } 

        private async ValueTask<IOutput> ValidateDuplicateNameAsync(Setting setting, CancellationToken cancellationToken)
        {
            var getSettingByNameResult = await settingsRepository.GetByNameAsync(setting.Name, cancellationToken);
            if (getSettingByNameResult.IsSuccessful)
            {
                var isCreate = setting.Id == 0;
                var duplicateFound = isCreate
                    ? getSettingByNameResult.Value.Any()
                    : getSettingByNameResult.Value.Any(currentSetting => currentSetting.Id != setting.Id);
                return duplicateFound
                    ? outputFactory.Conflict($"A setting with the name {setting.Name} already exists.", Constants.OriginSource)
                    : getSettingByNameResult;
            }

            return getSettingByNameResult;
        }

        #endregion
    }
}
