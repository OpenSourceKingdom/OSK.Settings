using OSK.Extensions.Settings.Local.Models;
using OSK.Extensions.Settings.Providers.Ports;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Settings.Abstractions;
using OSK.Storage.Local.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Extensions.Settings.Local
{
    internal class LocalStoragSettingValueProvider(string filePath, int rank, ILocalStorageService storageService,
        IOutputFactory<LocalStoragSettingValueProvider> outputFactory)
        : ISettingValueProvider
    {
        #region Variables

        private bool _hasInitialized;
        private IDictionary<long, LocalSettingValue> _localSettingValues = new Dictionary<long, LocalSettingValue>();

        #endregion

        #region ISettingValueProvider

        public int Rank => rank;

        public async Task<IOutput> DeleteAsync(long settingId, CancellationToken cancellationToken = default)
        {
            if (!_hasInitialized)
            {
                var initializeResult = await InitializeAsync(cancellationToken);
                if (!initializeResult.IsSuccessful)
                {
                    return initializeResult;
                }
            }

            var saveResult = await storageService.SaveAsync(new LocalSettings()
            {
                Settings = _localSettingValues.Values
            }, filePath, cancellationToken: cancellationToken);
            if (saveResult.IsSuccessful)
            {
                _localSettingValues.Remove(settingId);
            }

            return saveResult;
        }

        public async Task<IOutput<IEnumerable<SettingValue>>> GetSettingValuesAsync(CancellationToken cancellationToken = default)
        {
            if (!_hasInitialized)
            {
                var initializeResult = await InitializeAsync(cancellationToken);
                if (!initializeResult.IsSuccessful)
                {
                    return initializeResult.AsType<IEnumerable<SettingValue>>();
                }
            }

            return outputFactory.Success(_localSettingValues.Values.Select(ToSettingValue));
        }

        public async Task<IOutput<IEnumerable<SettingValue>>> UpsertValuesAsync(IEnumerable<SettingValue> settingValues, CancellationToken cancellationToken = default)
        {
            if (settingValues == null)
            {
                throw new ArgumentNullException(nameof(settingValues));
            }

            if (!_hasInitialized)
            {
                var initializeResult = await InitializeAsync(cancellationToken);
                if (!initializeResult.IsSuccessful)
                {
                    return initializeResult.AsType<IEnumerable<SettingValue>>();
                }
            }

            var updatedSettings = _localSettingValues.Select(currentSetting => new LocalSettingValue()
            {
                SettingId = currentSetting.Value.SettingId,
                SettingType = currentSetting.Value.SettingType,
                Value = currentSetting.Value.Value
            }).ToDictionary(item => item.SettingId);
            foreach (var setting in settingValues)
            {
                var typeAndStringValueTuple = ToTypeAndStringTuple(setting);
                updatedSettings[setting.SettingId] = new LocalSettingValue()
                {
                    SettingId = setting.SettingId,
                    SettingType = typeAndStringValueTuple.Item1,
                    Value = typeAndStringValueTuple.Item2
                };
            }

            var saveResult = await storageService.SaveAsync(new LocalSettings()
            {
                Settings = updatedSettings.Values
            }, filePath, cancellationToken: cancellationToken);
            if (saveResult.IsSuccessful)
            {
                _localSettingValues = updatedSettings;
            }

            return saveResult.IsSuccessful
                ? outputFactory.Success(settingValues)
                : saveResult.AsType<IEnumerable<SettingValue>>();
        }

        #endregion

        #region Helpers

        private async Task<IOutput> InitializeAsync(CancellationToken cancellationToken)
        {
            _hasInitialized = false;
            var getSettingsResult = await storageService.GetAsync(filePath, cancellationToken);
            if (!getSettingsResult.IsSuccessful)
            {
                return getSettingsResult;
            }

            try
            {
                var localSettings = await getSettingsResult.Value.StreamAsAsync<LocalSettings>(cancellationToken);
                _localSettingValues = localSettings.Settings.ToDictionary(settingValue => settingValue.SettingId);
                _hasInitialized = true;

                return outputFactory.Success(localSettings);
            }
            catch (Exception ex)
            {
                return outputFactory.Exception(ex, "OSK.Extensions.Settings.Local");
            }
        }

        protected virtual (int, string) ToTypeAndStringTuple(SettingValue settingValue) => settingValue switch
        {
            SettingValue<bool> boolean => (0, boolean.Value.ToString()),
            SettingValue<int> integer => (1, integer.Value.ToString()),
            SettingValue<float> f => (2, f.Value.ToString()),
            SettingValue<DateTime> dateTime => (3, dateTime.Value.ToString()),
            SettingValue<string> str => (4, str.Value),
            _ => throw new InvalidOperationException($"Setting value of type {settingValue.GetType().FullName} is  not currently convertible to a local setting value.")
        };

        protected virtual SettingValue ToSettingValue(LocalSettingValue localSettingValue) => localSettingValue.SettingType switch
        {
            0 => new SettingValue<bool>() { SettingId = localSettingValue.SettingId, Value = bool.Parse(localSettingValue.Value) },
            1 => new SettingValue<int>() { SettingId = localSettingValue.SettingId, Value = int.Parse(localSettingValue.Value) },
            2 => new SettingValue<float>() { SettingId = localSettingValue.SettingId, Value = float.Parse(localSettingValue.Value) },
            3 => new SettingValue<DateTime>() { SettingId = localSettingValue.SettingId, Value = DateTime.Parse(localSettingValue.Value) },
            4 => new SettingValue<string>() { SettingId = localSettingValue.SettingId, Value = localSettingValue.Value },
            _ => throw new ArgumentNullException($"Local setting value of type {localSettingValue.SettingType}, with value {localSettingValue.Value}, is not currently convertible to a setting value.")
        };

        #endregion
    }
}
