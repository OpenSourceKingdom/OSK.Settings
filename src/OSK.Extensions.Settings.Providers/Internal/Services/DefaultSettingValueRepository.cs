using OSK.Extensions.Settings.Providers.Ports;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Settings.Abstractions;
using OSK.Settings.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Extensions.Settings.Providers.Internal.Services
{
    internal class DefaultSettingValueRepository(IEnumerable<ISettingValueProvider> valueProviders,
        IOutputFactory<DefaultSettingValueRepository> outputFactory) : ISettingValueRepository
    {
        #region Variables

        private bool _hasInitialized;
        private IDictionary<long, Tuple<ISettingValueProvider, SettingValue>> _settingValueLookup = new Dictionary<long, Tuple<ISettingValueProvider, SettingValue>>();

        #endregion

        #region ISettingValueRepository

        public async Task<IOutput<SettingValue>> CreateAsync(SettingValue settingValue, CancellationToken cancellationToken = default)
        {
            if (settingValue == null)
            {
                throw new ArgumentNullException(nameof(settingValue));
            }

            var settingValueProvider = valueProviders.FirstOrDefault();
            if (settingValueProvider == null)
            {
                return outputFactory.Error<SettingValue>(HttpStatusCode.InternalServerError, $"No setting value handlers were available to create the setting", Constants.OriginSource);
            }

            var upsertResult = await settingValueProvider.UpsertValuesAsync([settingValue], cancellationToken);
            if (upsertResult.IsSuccessful)
            {
                _settingValueLookup[settingValue.SettingId] = new Tuple<ISettingValueProvider, SettingValue>(settingValueProvider, settingValue);
                return outputFactory.Success(settingValue);
            }

            return upsertResult.AsType<SettingValue>();
        }

        public async Task<IOutput<SettingValue>> UpdateAsync(SettingValue settingValue, CancellationToken cancellationToken = default)
        {
            if (settingValue == null)
            {
                throw new ArgumentNullException(nameof(settingValue));
            }
            if (!_hasInitialized)
            {
                var initializeResult = await InitializeAsync(cancellationToken);
                if (!initializeResult.IsSuccessful)
                {
                    return initializeResult.AsType<SettingValue>();
                }
            }
            if (!_settingValueLookup.TryGetValue(settingValue.SettingId, out var settingValueProviderTuple))
            {
                return outputFactory.NotFound<SettingValue>($"Setting value with id {settingValue.SettingId} not found.", Constants.OriginSource);
            }

            var upsertResult = await settingValueProviderTuple.Item1.UpsertValuesAsync([settingValue], cancellationToken);
            if (upsertResult.IsSuccessful)
            {
                _settingValueLookup[settingValue.SettingId] = new Tuple<ISettingValueProvider, SettingValue>(settingValueProviderTuple.Item1, settingValue);
                return outputFactory.Success(settingValue);
            }

            return upsertResult.AsType<SettingValue>();
        }

        public async Task<IOutput> DeleteAsync(long settngId, CancellationToken cancellationToken = default)
        {
            if (!_hasInitialized)
            {
                var initializeResult = await InitializeAsync(cancellationToken);
                if (!initializeResult.IsSuccessful)
                {
                    return initializeResult.AsType<SettingValue>();
                }
            }
            if (!_settingValueLookup.TryGetValue(settngId, out var settingValueTupleProvider))
            {
                return outputFactory.Success();
            }

            var deleteResult = await settingValueTupleProvider.Item1.DeleteAsync(settngId, cancellationToken);
            if (deleteResult.IsSuccessful)
            {
                _settingValueLookup.Remove(settngId);
            }

            return deleteResult;
        }

        public async Task<IOutput<SettingValue>> GetAsync(long settingId, CancellationToken cancellationToken = default)
        {
            if (!_hasInitialized)
            {
                var initializeResult = await InitializeAsync(cancellationToken);
                if (!initializeResult.IsSuccessful)
                {
                    return initializeResult.AsType<SettingValue>();
                }
            }
            if (!_settingValueLookup.TryGetValue(settingId, out var settingValueProviderTuple))
            {
                return outputFactory.NotFound<SettingValue>($"Setting with id {settingId} was not found.", Constants.OriginSource);
            }

            return outputFactory.Success(settingValueProviderTuple.Item2);
        }

        public async Task<IOutput<IEnumerable<SettingValue>>> GetSettingValuesByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
        {
            if (!_hasInitialized)
            {
                var initializeResult = await InitializeAsync(cancellationToken);
                if (!initializeResult.IsSuccessful)
                {
                    return initializeResult.AsType<IEnumerable<SettingValue>>();
                }
            }

            var settingValues = ids.Where(id => _settingValueLookup.TryGetValue(id, out _))
                                   .Select(id => _settingValueLookup[id].Item2);
            return outputFactory.Success(settingValues);
        }

        public async Task<IOutput<IEnumerable<SettingValue>>> UpdateSettingValuesAsync(IEnumerable<SettingValue> settingValues, CancellationToken cancellationToken = default)
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

            var settingTupleGroups = settingValues.Select(settingValue =>
            {
                ISettingValueProvider? settingValueProvider;
                if (_settingValueLookup.TryGetValue(settingValue.SettingId, out var settingValueTupleProvider))
                {
                    settingValueProvider = settingValueTupleProvider.Item1;
                }
                else
                {
                    settingValueProvider = _settingValueLookup.Values.FirstOrDefault()?.Item1;
                    if (settingValueProvider == null)
                    {
                        throw new InvalidOperationException($"No setting value handlers were available to create the setting");
                    }
                }

                return new { SettingValueProvider = settingValueProvider, SettingValue = settingValue };
            }).GroupBy(item => item.SettingValueProvider);

            foreach (var group in settingTupleGroups)
            {
                var updateResult = await group.Key.UpsertValuesAsync(group.Select(item => item.SettingValue), cancellationToken);
                if (!updateResult.IsSuccessful)
                {
                    return updateResult;
                }

                foreach (var item in group)
                {
                    _settingValueLookup[item.SettingValue.SettingId] = new Tuple<ISettingValueProvider, SettingValue>(group.Key, item.SettingValue);
                }
            }

            return outputFactory.Success(settingValues);
        }

        #endregion

        #region Helpers

        internal async ValueTask<IOutput> InitializeAsync(CancellationToken cancellationToken)
        {
            _hasInitialized = false;
            _settingValueLookup.Clear();

            // Flow from higher to lower ranking (Global -> Local) settings to override the settings above it
            foreach (var provider in valueProviders.OrderByDescending(p => p.Rank))
            {
                var getSettingValuesResult = await provider.GetSettingValuesAsync(cancellationToken);
                if (!getSettingValuesResult.IsSuccessful)
                {
                    return getSettingValuesResult;
                }

                foreach (var settingValue in getSettingValuesResult.Value)
                {
                    _settingValueLookup[settingValue.SettingId] = new Tuple<ISettingValueProvider, SettingValue>(provider, settingValue);
                }
            }

            _hasInitialized = true;
            return outputFactory.Success();
        }

        #endregion
    }
}
