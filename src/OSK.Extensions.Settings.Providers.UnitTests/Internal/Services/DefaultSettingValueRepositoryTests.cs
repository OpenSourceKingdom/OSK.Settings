using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Moq;
using OSK.Extensions.Settings.Providers.Internal.Services;
using OSK.Extensions.Settings.Providers.Ports;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Functions.Outputs.Mocks;
using OSK.Settings.Abstractions;
using OSK.Settings.Abstractions.Settings;
using System.Net;
using Xunit;

namespace OSK.Extensions.Settings.Providers.UnitTests.Internal.Services
{
    public class DefaultSettingValueRepositoryTests
    {
        #region Variables

        private readonly Mock<ISettingValueProvider> _globalSettingProvider;
        private readonly Mock<ISettingValueProvider> _localSettingProvider;
        private readonly IOutputFactory<DefaultSettingValueRepository> _outputFactory;

        private readonly DefaultSettingValueRepository _repository;

        #endregion

        #region Constructors

        public DefaultSettingValueRepositoryTests()
        {
            _globalSettingProvider = new Mock<ISettingValueProvider>();
            _globalSettingProvider.SetupGet(m => m.Rank)
                .Returns(2);

            _localSettingProvider = new Mock<ISettingValueProvider>();
            _localSettingProvider.SetupGet(m => m.Rank)
                .Returns(1);

            _outputFactory = new MockOutputFactory<DefaultSettingValueRepository>();

            _repository = new DefaultSettingValueRepository(new List<ISettingValueProvider>()
            {
                _localSettingProvider.Object, _globalSettingProvider.Object
            }, _outputFactory);
        }

        #endregion

        #region InitializeAsync

        [Fact]
        public async Task InitializeAsync_SettingProviderReturnsError_ReturnsError()
        {
            // Arrange
            _globalSettingProvider.Setup(m => m.GetSettingValuesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Error<IEnumerable<SettingValue>>(HttpStatusCode.InternalServerError, "A bad day"));

            // Act
            var result = await _repository.InitializeAsync(CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.InternalServerError, result.Code.StatusCode);
        }

        [Fact]
        public async Task InitializeAsync_ValidSettingValues_ReturnsSuccessful()
        {
            // Arrange
            SetupSettingValuesResponse(_globalSettingProvider, Enumerable.Empty<SettingValue>().ToArray());
            SetupSettingValuesResponse(_localSettingProvider, new SettingValue<int>());

            // Act
            var result = await _repository.InitializeAsync(CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccessful);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_NullSettingValue_ThrowsArgumentNullException()
        {
            // Arrange/Act/Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.CreateAsync(null));
        }

        [Fact]
        public async Task CreateAsync_NoSettingValueProviders_ReturnsInternalServerError()
        {
            // Arrange
            var repository = new DefaultSettingValueRepository(Enumerable.Empty<ISettingValueProvider>(), _outputFactory);

            // Act
            var result = await repository.CreateAsync(new SettingValue<int>());

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.InternalServerError, result.Code.StatusCode);
        }

        [Fact]
        public async Task CreateAsync_ValidSettingValueProviderReturnsError_ReturnsError()
        {
            // Arrange
            _localSettingProvider.Setup(m
                => m.UpsertValuesAsync(It.IsAny<IEnumerable<SettingValue>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Error<IEnumerable<SettingValue>>(HttpStatusCode.BadGateway, "A bad day"));


            // Act
            var result = await _repository.CreateAsync(new SettingValue<int>());

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.BadGateway, result.Code.StatusCode);
        }

        [Fact]
        public async Task CreateAsync_ValidSettingValueProvider_ReturnsSuccessfully()
        {
            // Arrange
            _localSettingProvider.Setup(m
                => m.UpsertValuesAsync(It.IsAny<IEnumerable<SettingValue>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<SettingValue> settingValues, CancellationToken _)
                    => _outputFactory.Success(settingValues));

            // Act
            var result = await _repository.CreateAsync(new SettingValue<int>());

            // Assert
            Assert.True(result.IsSuccessful);

            _localSettingProvider.Verify(m => m.UpsertValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()), Times.Once);
            _globalSettingProvider.Verify(m => m.UpsertValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_NullSettingValue_ThrowsArgumentNullException()
        {
            // Arrange/Act/Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.UpdateAsync(null));
        }

        [Fact]
        public async Task UpdateAsync_NoSettingValueProvidersAssociatedWithSettingId_ReturnsNotFound()
        {
            // Arrange
            SetupSettingValuesResponse(_globalSettingProvider);
            SetupSettingValuesResponse(_localSettingProvider);

            // Act
            var result = await _repository.UpdateAsync(new SettingValue<int>());

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.NotFound, result.Code.StatusCode);
        }

        [Fact]
        public async Task UpdateAsync_SettingValueProvidersReturnsError_ReturnsError()
        {
            // Arrange
            var setting = new SettingValue<int>();

            SetupSettingValuesResponse(_globalSettingProvider, setting);
            SetupSettingValuesResponse(_localSettingProvider, setting);

            _localSettingProvider.Setup(m => m.UpsertValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Error<IEnumerable<SettingValue>>(HttpStatusCode.Forbidden, "A bad day"));

            // Act
            var result = await _repository.UpdateAsync(setting);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.Forbidden, result.Code.StatusCode);

            _localSettingProvider.Verify(m => m.UpsertValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()), Times.Once);
            _globalSettingProvider.Verify(m => m.UpsertValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_SettingValueProvidersReturnsSuccessfully_ReturnsSuccessfully()
        {
            // Arrange
            var setting = new SettingValue<int>();

            SetupSettingValuesResponse(_globalSettingProvider, setting);
            SetupSettingValuesResponse(_localSettingProvider);

            _globalSettingProvider.Setup(m => m.UpsertValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<SettingValue> values, CancellationToken _) 
                    => _outputFactory.Success(values));

            // Act
            var result = await _repository.UpdateAsync(setting);

            // Assert
            Assert.True(result.IsSuccessful);

            _localSettingProvider.Verify(m => m.UpsertValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()), Times.Never);
            _globalSettingProvider.Verify(m => m.UpsertValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region UpdateSettingValuesAsync

        [Fact]
        public async Task UpdateSettingValuesAsync_NullSettings_ThrowsArgumentNullException()
        {
            // Arrange/Act/Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.UpdateSettingValuesAsync(null));
        }

        [Fact]
        public async Task UpdateSettingValuesAsync_NoSettingValueProviders_ThrowsInvalidOperationException()
        {
            // Arrange
            var repository = new DefaultSettingValueRepository(Enumerable.Empty<ISettingValueProvider>(), _outputFactory);

            // Act/Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpdateSettingValuesAsync([new SettingValue<int>()]));
        }

        [Fact]
        public async Task UpdateSettingValuesAsync_SettingValueProviderReturnsError_ReturnsError()
        {
            // Arrange
            var settingValue = new SettingValue<int>();

            SetupSettingValuesResponse(_globalSettingProvider, settingValue);
            SetupSettingValuesResponse(_localSettingProvider, settingValue);

            _localSettingProvider.Setup(m => m.UpsertValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Error<IEnumerable<SettingValue>>(HttpStatusCode.InternalServerError, "A bad day"));

            // Act
            var result = await _repository.UpdateSettingValuesAsync([settingValue]);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.InternalServerError, result.Code.StatusCode);

            _localSettingProvider.Verify(m => m.UpsertValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()), Times.Once);
            _globalSettingProvider.Verify(m => m.UpsertValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateSettingValuesAsync_UpdateVariousSettingValues_ReturnsSuccessfully()
        {
            // Arrange
            var settingValue = new SettingValue<int>()
            {
                SettingId = 1
            };
            var settingValueWinner = new SettingValue<int>()
            {
                SettingId = 1
            };
            var settingValue1 = new SettingValue<int>()
            {
                SettingId = 2
            };
            var settingValue2 = new SettingValue<int>()
            {
                SettingId = 3
            };
            var settingValue3 = new SettingValue<int>()
            {
                SettingId = 4
            };

            SetupSettingValuesResponse(_globalSettingProvider, settingValue, settingValue2);
            SetupSettingValuesResponse(_localSettingProvider, settingValueWinner, settingValue1);

            _globalSettingProvider.Setup(m => m.UpsertValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<SettingValue> values, CancellationToken _) =>
                    _outputFactory.Success(values));
            _localSettingProvider.Setup(m => m.UpsertValuesAsync(It.IsAny<IEnumerable<SettingValue>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<SettingValue> values, CancellationToken _) =>
                    _outputFactory.Success(values));

            // Act
            var result = await _repository.UpdateSettingValuesAsync([settingValue, settingValue1, settingValue2, settingValue3]);

            // Assert
            Assert.True(result.IsSuccessful);

            // Expecting 3:
            // Value wins against global.
            // One setting specific to this provider
            // First provider for upsert on new setting
            _localSettingProvider.Verify(m => m.UpsertValuesAsync(It.Is<IEnumerable<SettingValue>>(
                e => e.Count() == 3),
                It.IsAny<CancellationToken>()), Times.Once);
            
            // Expecting 1:
            // Value loses against local
            // Value specific to provider
            // Loses to local provider on new setting
            _globalSettingProvider.Verify(m => m.UpsertValuesAsync(It.Is<IEnumerable<SettingValue>>(
                e => e.Count() == 1),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region GetAsync

        [Fact]
        public async Task GetAsync_SettingIdDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var setting = new SettingValue<int>();

            SetupSettingValuesResponse(_globalSettingProvider, setting);
            SetupSettingValuesResponse(_localSettingProvider, setting);

            // Act
            var result = await _repository.GetAsync(1);

            // Assert
            Assert.False(result.IsSuccessful);
        }

        [Fact]
        public async Task GetAsync_SettingIdExists_ReturnsValueFromProvider()
        {
            // Arrange

            var setting = new SettingValue<int>()
            {
                SettingId = 11
            };
            var setting2 = new SettingValue<int>()
            {
                SettingId = 11
            };

            SetupSettingValuesResponse(_globalSettingProvider, setting);
            SetupSettingValuesResponse(_localSettingProvider, setting2);

            // Act
            var result = await _repository.GetAsync(setting2.SettingId);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal(setting2, result.Value);
        }

        #endregion

        #region GetSettingValuesByIdsAsync

        [Fact]
        public async Task GetSettingValuesByIdsAsync_NoSettingIdsMatch_ReturnsEmptyList()
        {
            // Arrange
            SetupSettingValuesResponse(_globalSettingProvider);
            SetupSettingValuesResponse(_localSettingProvider);

            // Act
            var result = await _repository.GetSettingValuesByIdsAsync([1, 2, 3, 4, 5]);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetSettingValuesByIdsAsync_VariousIds_SomeMatchSomeDont_ReturnsExpectedList()
        {
            // Arrange
            var settingValue1 = new SettingValue<int>()
            {
                SettingId = 1
            };
            var settingValue1Winner = new SettingValue<int>()
            {
                SettingId = 1
            };
            var settingValue2 = new SettingValue<int>()
            {
                SettingId = 2
            };
            var settingValue3 = new SettingValue<int>()
            {
                SettingId = 3
            };

            SetupSettingValuesResponse(_globalSettingProvider, settingValue1, settingValue2);
            SetupSettingValuesResponse(_localSettingProvider, settingValue1Winner, settingValue3);

            // Act
            var result = await _repository.GetSettingValuesByIdsAsync([1, 2, 3, 4, 5]);

            // Assert
            Assert.True(result.IsSuccessful);

            var valueList = result.Value.ToList();
            Assert.Equal(3, valueList.Count);

            Assert.Equal(settingValue1Winner, valueList[0]);
            Assert.Equal(settingValue2, valueList[1]);
            Assert.Equal(settingValue3, valueList[2]);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_SettingProviderReturnsError_ReturnsError()
        {
            // Arrange
            var setting = new SettingValue<int>()
            {
                SettingId = 32
            };

            SetupSettingValuesResponse(_globalSettingProvider, setting);
            SetupSettingValuesResponse(_localSettingProvider, setting);

            _localSettingProvider.Setup(m => m.DeleteAsync(It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Error(HttpStatusCode.BadRequest, "A bad day"));

            // Act
            var result = await _repository.DeleteAsync(setting.SettingId);

            // Assert
            Assert.False(result.IsSuccessful);

            _localSettingProvider.Verify(m => m.DeleteAsync(It.IsAny<long>(),
                It.IsAny<CancellationToken>()), Times.Once);
            _globalSettingProvider.Verify(m => m.DeleteAsync(It.IsAny<long>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_MissingSettingId_ReturnsSuccessfully()
        {
            // Arrange
            SetupSettingValuesResponse(_globalSettingProvider);
            SetupSettingValuesResponse(_localSettingProvider);

            // Act
            var result = await _repository.DeleteAsync(1);

            // Assert
            Assert.True(result.IsSuccessful);

            _localSettingProvider.Verify(m => m.DeleteAsync(It.IsAny<long>(),
                It.IsAny<CancellationToken>()), Times.Never);
            _globalSettingProvider.Verify(m => m.DeleteAsync(It.IsAny<long>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_BothSettingProvidersHaveSetting_LowestRankProviderPrevails_DeletesSuccessfully_ReturnsSuccessfully()
        {
            // Arrange
            var setting = new SettingValue<int>()
            {
                SettingId = 32
            };

            SetupSettingValuesResponse(_globalSettingProvider, setting);
            SetupSettingValuesResponse(_localSettingProvider, setting);

            _localSettingProvider.Setup(m => m.DeleteAsync(It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success());

            // Act
            var result = await _repository.DeleteAsync(setting.SettingId);

            // Assert
            Assert.True(result.IsSuccessful);

            _localSettingProvider.Verify(m => m.DeleteAsync(It.IsAny<long>(),
                It.IsAny<CancellationToken>()), Times.Once);
            _globalSettingProvider.Verify(m => m.DeleteAsync(It.IsAny<long>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region Helpers

        private void SetupSettingValuesResponse(Mock<ISettingValueProvider> provider, params SettingValue[] settingValues)
        {
            provider.Setup(m => m.GetSettingValuesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((IEnumerable<SettingValue>)settingValues));
        }

        #endregion
    }
}
