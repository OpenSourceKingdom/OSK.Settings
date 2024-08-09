using Moq;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Functions.Outputs.Mocks;
using OSK.Settings.Abstractions;
using OSK.Settings.Abstractions.Settings;
using OSK.Settings.Management.Internal.Services;
using OSK.Settings.Management.Models;
using OSK.Settings.Models;
using OSK.Settings.Ports;
using System.Net;
using Xunit;

namespace OSK.Settings.Management.UnitTests.Internal.Services
{
    public class DefaultSettingsManagerTests
    {
        #region Variables

        private readonly Mock<ISettingsService> _mockSettingsService;
        private readonly Mock<ISettingValueRepository> _mockSettingValueRepository;
        private readonly IOutputFactory<DefaultSettingsManager> _outputFactory;

        private readonly DefaultSettingsManager _manager;

        #endregion

        #region Constructors

        public DefaultSettingsManagerTests()
        {
            _mockSettingsService = new Mock<ISettingsService>();
            _mockSettingValueRepository = new Mock<ISettingValueRepository>();

            _outputFactory = new MockOutputFactory<DefaultSettingsManager>();

            _manager = new DefaultSettingsManager(_mockSettingsService.Object, _mockSettingValueRepository.Object,
                _outputFactory);
        }

        #endregion

        #region GetEffectiveSettingAsync

        [Fact]
        public async Task GetEffectiveSettingAsync_CallsThroughToService()
        {
            // Arrange
            var effectiveSetting = new EffectiveSetting<int>(new IntegerSetting() { Id = 4 }, 1);
            _mockSettingsService.Setup(m => m.GetEffectiveSettingAsync<int>(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success(effectiveSetting));

            // Act
            var result = await _manager.GetEffectiveSettingAsync<int>(1);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal(effectiveSetting, result.Value);
            _mockSettingsService.Verify(m => m.GetEffectiveSettingAsync<int>(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region ResetStagedUpdates

        [Fact]
        public void ResetStagedUpdates_NothingStaged_ClearsWithoutError()
        {
            // Arrange/Act/Assert
            _manager.ResetStagedUpdates();
        }


        [Fact]
        public void ResetStagedUpdates_HasStagedUpdated_ClearsWithoutError()
        {
            // Arrange
            _manager._stagedUpdates = new Dictionary<long, Models.ManagedSetting>()
            {
                { 1, new ManagedSetting(null, null) }
            };
            
            // Act/Assert
            _manager.ResetStagedUpdates();
            Assert.Empty(_manager._stagedUpdates);
        }

        #endregion

        #region ApplySettingsAsync

        [Fact]
        public async Task ApplySettingsAsync_NothingStaged_NoRepositoryCall_ReturnsSuccessfully()
        {
            // Arrange
            _manager._stagedUpdates = new Dictionary<long, ManagedSetting>();

            // Act
            var result = await _manager.ApplySettingsAsync();

            // Assert
            Assert.True(result.IsSuccessful);
            _mockSettingValueRepository.Verify(m => m.UpdateSettingValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ApplySettingsAsync_MultipleStagedUpdated_SingleSuccessfulRepositoryCall_ResetsStagedUpdates_ReturnsSuccessfully()
        {
            // Arrange
            _manager._stagedUpdates = new Dictionary<long, ManagedSetting>() 
            {
                { 1, new ManagedSetting(null, null) },
                { 2, new ManagedSetting(null, null) }
            };

            _mockSettingValueRepository.Setup(m => m.UpdateSettingValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<SettingValue> values, CancellationToken _) => _outputFactory.Success(values));
            
            // Act
            var result = await _manager.ApplySettingsAsync();

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Empty(_manager._stagedUpdates);
            _mockSettingValueRepository.Verify(m => m.UpdateSettingValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ApplySettingsAsync_MultipleStagedUpdated_SingleFailedRepositoryCall_DoesNotResetStagedUpdates_ReturnsError()
        {
            // Arrange
            _manager._stagedUpdates = new Dictionary<long, ManagedSetting>()
            {
                { 1, new ManagedSetting(null, null) },
                { 2, new ManagedSetting(null, null) }
            };

            _mockSettingValueRepository.Setup(m => m.UpdateSettingValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.BadRequest<IEnumerable<SettingValue>>("Bad Day"));

            // Act
            var result = await _manager.ApplySettingsAsync();

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(2, _manager._stagedUpdates.Count);
            _mockSettingValueRepository.Verify(m => m.UpdateSettingValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region GetSettingsByPageAsync

        [Fact]
        public async Task GetSettingsByPageAsync_CallToGetSettingValuePairsFails_ReturnsError()
        {
            // Arrange
            _mockSettingsService.Setup(m => m.GetSettingValuePairsAsync(It.IsAny<SettingCategory?>(), It.IsAny<long>(),
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.BadRequest<PaginatedOutput<SettingValuePair>>("Bad Day"));

            // Act
            var result = await _manager.GetSettingsByPageAsync(null, 1, 100);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.BadRequest, result.Code.StatusCode);
        }

        [Fact]
        public async Task GetSettingsByPageAsync_CallToGetSettingValuePairsSucceeds_ReturnsSuccessfully()
        {
            // Arrange
            var testPairs = new List<SettingValuePair>()
            {
                new SettingValuePair<int>()
                {
                    Setting = new IntegerSetting()
                    {
                        Id = 7
                    }
                },
                new SettingValuePair<float>()
                {
                    Setting = new FloatSetting()
                    {
                        Id = 6
                    }
                },
            };

            _mockSettingsService.Setup(m => m.GetSettingValuePairsAsync(It.IsAny<SettingCategory?>(), It.IsAny<long>(),
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SettingCategory _, long skip, long take, CancellationToken _)
                 =>_outputFactory.Success(new PaginatedOutput<SettingValuePair>()
                    {
                        Items = testPairs,
                        Skip = skip,
                        Take = take,
                        Total = testPairs.Count
                    }));

            // Act
            var result = await _manager.GetSettingsByPageAsync(null, 1, 100);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal(1, result.Value.Skip);
            Assert.Equal(100, result.Value.Take);
            Assert.Equal(testPairs.Count, result.Value.Total);

            Assert.All(result.Value.Items, item => testPairs.Any(pair => pair.GetSetting().Id == item.GetSetting().Id));
        }

        #endregion

        #region StageSettingUpdate

        [Fact]
        public void StageSettingUpdate_NullSetting_ThrowsArgumentNullException()
        {
            // Arrange/Act/Assert
            Assert.Throws<ArgumentNullException>(() => _manager.StageSettingUpdate(null, 9));
        }

        [Fact]
        public void StageSettingUpdate_SettingValidationFailsOnValue_ReturnsError()
        {
            // Arrange
            var managedSetting = new ManagedSetting(new SettingValuePair<int>()
            {
                Setting = new IntegerSetting()
                {
                    MinValue = 9
                },
                Value = 11
            }, _manager);

            // Act
            var result = _manager.StageSettingUpdate(managedSetting, -9);

            // Assert
            Assert.False(result.IsSuccessful);
        }

        [Fact]
        public void StageSettingUpdate_SettingPassesValidation_AddsToStaging_ReturnsSuccessfully()
        {
            // Arrange
            _manager._stagedUpdates.Clear();

            var managedSetting = new ManagedSetting(new SettingValuePair<int>()
            {
                Setting = new IntegerSetting()
                {
                    MinValue = 9
                },
                Value = 11
            }, _manager);

            // Act
            var result = _manager.StageSettingUpdate(managedSetting, 9);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Single(_manager._stagedUpdates);
            Assert.Equal(managedSetting, _manager._stagedUpdates.First().Value);
        }

        #endregion
    }
}
