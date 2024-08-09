using Moq;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Functions.Outputs.Mocks;
using OSK.Settings.Abstractions;
using OSK.Settings.Abstractions.Settings;
using OSK.Settings.Internal.Services;
using OSK.Settings.Ports;
using System.Net;
using Xunit;

namespace OSK.Settings.UnitTests.Internal.Services
{
    public class SettingsServiceTests
    {
        #region Variables

        private readonly Mock<ISettingsRepository> _mockSettingsRepository;
        private readonly Mock<ISettingValueRepository> _mockSettingValueRepository;
        private readonly IOutputFactory<SettingsService> _outputFactory;

        private readonly ISettingsService _service;

        #endregion

        #region Constructors

        public SettingsServiceTests()
        {
            _mockSettingsRepository = new Mock<ISettingsRepository>();
            _mockSettingValueRepository = new Mock<ISettingValueRepository>();
            _outputFactory = new MockOutputFactory<SettingsService>();

            _service = new SettingsService(_mockSettingsRepository.Object, _mockSettingValueRepository.Object,
                _outputFactory);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_NullSetting_ThrowsArgumentNullException()
        {
            // Arrange/Act/Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(null));
        }

        [Fact]
        public async Task CreateAsync_SettingParametersAreNotValid_ReturnsBadRequest()
        {
            // Arrange
            var setting = new StringSetting()
            {
                Name = "Hello World",
                MaxCharacters = 2,
                MinCharacters = 3
            };

            // Act
            var result = await _service.CreateAsync(setting);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.BadRequest, result.Code.StatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("     ")]
        public async Task CreateAsync_InvalidSettingName_ReturnsBadRequest(string name)
        {
            // Arrange
            var setting = new StringSetting()
            {
                Name = name
            };

            // Act
            var result = await _service.CreateAsync(setting);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.BadRequest, result.Code.StatusCode);
        }

        [Fact]
        public async Task CreateAsync_GetByNameCallReturnsError_ReturnsError()
        {
            // Arrange
            var setting = new StringSetting()
            {
                Id = 3,
                Name = "Hello world"
            };

            _mockSettingsRepository.Setup(m => m.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Error<IEnumerable<Setting>>(HttpStatusCode.InternalServerError, "Hello"));

            // Act
            var result = await _service.CreateAsync(setting);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.InternalServerError, result.Code.StatusCode);
        }

        [Fact]
        public async Task CreateAsync_DuplicateSettingName_ReturnsConflict()
        {
            // Arrange
            var setting = new StringSetting()
            {
                Id = 3,
                Name = "Hello world"
            };

            _mockSettingsRepository.Setup(m => m.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((IEnumerable<Setting>)new List<Setting>() { new BooleanSetting() { Id = 1 } }));

            // Act
            var result = await _service.CreateAsync(setting);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.Conflict, result.Code.StatusCode);
        }

        [Fact]
        public async Task CreateAsync_Valid_ReturnsSuccessfully()
        {
            // Arrange
            var setting = new StringSetting()
            {
                Name = "Hello world"
            };

            _mockSettingsRepository.Setup(m => m.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success(Enumerable.Empty<Setting>()));
            _mockSettingsRepository.Setup(m => m.CreateAsync(It.IsAny<Setting>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Setting setting, CancellationToken _) => _outputFactory.Success(setting));

            // Act
            var result = await _service.CreateAsync(setting);

            // Assert
            Assert.True(result.IsSuccessful);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_NullSetting_ThrowsArgumentNullException()
        {
            // Arrange/Act/Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateAsync(null));
        }

        [Fact]
        public async Task UpdateAsync_GetSettingReturnsError_ReturnsError()
        {
            // Arrange
            var setting = new StringSetting()
            {
                Name = "Hello World",
                MaxCharacters = 2,
                MinCharacters = 3
            };

            _mockSettingsRepository.Setup(m => m.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.NotFound<Setting>("Bad day"));

            // Act
            var result = await _service.UpdateAsync(setting);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.NotFound, result.Code.StatusCode);
        }

        [Fact]
        public async Task UpdateAsync_SettingParametersAreNotValid_ReturnsBadRequest()
        {
            // Arrange
            var setting = new StringSetting()
            {
                Name = "Hello World",
                MaxCharacters = 2,
                MinCharacters = 3
            };

            _mockSettingsRepository.Setup(m => m.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((Setting)setting));

            // Act
            var result = await _service.UpdateAsync(setting);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.BadRequest, result.Code.StatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("     ")]
        public async Task UpdateAsync_InvalidSettingName_ReturnsBadRequest(string name)
        {
            // Arrange
            var setting = new StringSetting()
            {
                Name = name
            };

            _mockSettingsRepository.Setup(m => m.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((Setting)setting));

            // Act
            var result = await _service.UpdateAsync(setting);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.BadRequest, result.Code.StatusCode);
        }

        [Fact]
        public async Task UpdateAsync_GetByNameCallReturnsError_ReturnsError()
        {
            // Arrange
            var setting = new StringSetting()
            {
                Id = 3,
                Name = "Hello world"
            };

            _mockSettingsRepository.Setup(m => m.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((Setting)setting));
            _mockSettingsRepository.Setup(m => m.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Error<IEnumerable<Setting>>(HttpStatusCode.InternalServerError, "Hello"));

            // Act
            var result = await _service.CreateAsync(setting);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.InternalServerError, result.Code.StatusCode);
        }

        [Fact]
        public async Task UpdateAsync_DuplicateSettingName_ReturnsConflict()
        {
            // Arrange
            var setting = new StringSetting()
            {
                Id = 3,
                Name = "Hello world"
            };

            _mockSettingsRepository.Setup(m => m.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((Setting)setting));
            _mockSettingsRepository.Setup(m => m.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((IEnumerable<Setting>)new List<Setting>() { new BooleanSetting() { Id = 1 } }));

            // Act
            var result = await _service.UpdateAsync(setting);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.Conflict, result.Code.StatusCode);
        }

        [Fact]
        public async Task UpdateAsync_Valid_ReturnsSuccessfully()
        {
            // Arrange
            var setting = new StringSetting()
            {
                Id = 4,
                Name = "Hello world"
            };

            _mockSettingsRepository.Setup(m => m.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((Setting)setting));
            _mockSettingsRepository.Setup(m => m.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((IEnumerable<Setting>)new List<Setting>() { setting }));
            _mockSettingsRepository.Setup(m => m.UpdateAsync(It.IsAny<Setting>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Setting setting, CancellationToken _) => _outputFactory.Success(setting));

            // Act
            var result = await _service.UpdateAsync(setting);

            // Assert
            Assert.True(result.IsSuccessful);
        }

        #endregion

        #region GetEffectiveSettingAsync

        [Fact]
        public async Task GetEffectiveSettingAsync_GetSettingReturnsError_ReturnsError()
        {
            // Arrange
            _mockSettingsRepository.Setup(m => m.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.NotFound<Setting>("Bad day"));

            // Act
            var result = await _service.GetEffectiveSettingAsync<int>(1);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.NotFound, result.Code.StatusCode);
        }

        [Fact]
        public async Task GetEffectiveSettingAsync_SettingDoesNotMatchExpectedSettingType_ReturnsBadRequest()
        {
            // Arrange
            var setting = new StringSetting();

            _mockSettingsRepository.Setup(m => m.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((Setting)new IntegerSetting()));

            // Act
            var result = await _service.GetEffectiveSettingAsync<string>(1);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.BadRequest, result.Code.StatusCode);
        }

        [Fact]
        public async Task GetEffectiveSettingAsync_GetSettingValueReturnsError_ReturnsError()
        {
            // Arrange
            var setting = new StringSetting();

            _mockSettingsRepository.Setup(m => m.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((Setting)setting));
            _mockSettingValueRepository.Setup(m => m.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Error<SettingValue>(HttpStatusCode.InternalServerError, "Bad day"));

            // Act
            var result = await _service.GetEffectiveSettingAsync<string>(1);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.InternalServerError, result.Code.StatusCode);
        }

        [Fact]
        public async Task GetEffectiveSettingAsync_SettingValueDoesNotMatchExpectedType_ReturnsBadRequest()
        {
            // Arrange
            var setting = new StringSetting();

            _mockSettingsRepository.Setup(m => m.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((Setting)setting));
            _mockSettingValueRepository.Setup(m => m.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((SettingValue)new SettingValue<int>()));

            // Act
            var result = await _service.GetEffectiveSettingAsync<string>(1);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.BadRequest, result.Code.StatusCode);
        }

        [Fact]
        public async Task GetEffectiveSettingAsync_SettingValueNotFound_ReturnsSettingDefaultSuccessfully()
        {
            // Arrange
            var setting = new StringSetting()
            {
                Id = 117,
                DefaultValue = "Hello"
            };

            _mockSettingsRepository.Setup(m => m.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((Setting)setting));
            _mockSettingValueRepository.Setup(m => m.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.NotFound<SettingValue>("A bad day"));

            // Act
            var result = await _service.GetEffectiveSettingAsync<string>(1);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal(setting, result.Value.Setting);
            Assert.Equal(setting.Id, result.Value.SettingId);
            Assert.Equal(setting.DefaultValue, result.Value.Value);
        }

        [Fact]
        public async Task GetEffectiveSettingAsync_Valid_ReturnsSuccessfully()
        {
            // Arrange
            var setting = new StringSetting()
            {
                Id = 117,
                DefaultValue = "Hello"
            };
            var settingValue = new SettingValue<string>()
            {
                SettingId = setting.Id,
                Value = "Hello"
            };

            _mockSettingsRepository.Setup(m => m.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((Setting)setting));
            _mockSettingValueRepository.Setup(m => m.GetAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((SettingValue)settingValue));

            // Act
            var result = await _service.GetEffectiveSettingAsync<string>(1);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal(setting, result.Value.Setting);
            Assert.Equal(setting.Id, result.Value.SettingId);
            Assert.Equal(settingValue.Value, result.Value.Value);
        }

        #endregion

        #region GetSettingValuePairsAsync

        [Fact]
        public async Task GetSettingValuePairsAsync_GetPageReturnsError_ReturnsError()
        {
            // Arrange
            _mockSettingsRepository.Setup(m => m.GetPageAsync(It.IsAny<SettingCategory?>(), It.IsAny<long>(),
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.BadRequest<PaginatedOutput<Setting>>("Bad"));

            // Act
            var result = await _service.GetSettingValuePairsAsync(null, 0, 100);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.BadRequest, result.Code.StatusCode);
        }

        [Fact]
        public async Task GetSettingValuePairsAsync_GetSettingsByIdsReturnsError_ReturnsError()
        {
            // Arrange
            _mockSettingsRepository.Setup(m => m.GetPageAsync(It.IsAny<SettingCategory?>(), It.IsAny<long>(),
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success(new PaginatedOutput<Setting>()
                {
                    Items = new List<Setting>()
                }));

            _mockSettingValueRepository.Setup(m => m.GetSettingValuesByIdsAsync(It.IsAny<IEnumerable<long>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Conflict<IEnumerable<SettingValue>>("a bad day"));

            // Act
            var result = await _service.GetSettingValuePairsAsync(null, 0, 100);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.Conflict, result.Code.StatusCode);
        }

        [Fact]
        public async Task GetSettingValuePairsAsync_SettingDoesNotMatchSettingValueType_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockSettingsRepository.Setup(m => m.GetPageAsync(It.IsAny<SettingCategory?>(), It.IsAny<long>(),
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success(new PaginatedOutput<Setting>()
                {
                    Items = new List<Setting>()
                    {
                        new StringSetting()
                        {
                            Id = 2
                        }
                    }
                }));

            _mockSettingValueRepository.Setup(m => m.GetSettingValuesByIdsAsync(It.IsAny<IEnumerable<long>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((IEnumerable<SettingValue>)new List<SettingValue>()
                {
                    new SettingValue<int>()
                    {
                        SettingId = 2
                    }
                }));

            // Act/Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetSettingValuePairsAsync(null, 0, 100));
        }

        [Fact]
        public async Task GetSettingValuePairsAsync_ValidAndMissingSettingValues_ReturnsExpectedAndDefaultSettingValuePairsSuccessfully()
        {
            // Arrange
            _mockSettingsRepository.Setup(m => m.GetPageAsync(It.IsAny<SettingCategory?>(), It.IsAny<long>(),
                It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success(new PaginatedOutput<Setting>()
                {
                    Items = new List<Setting>()
                    {
                        new IntegerSetting()
                        {
                            Id = 1,
                            DefaultValue = 23
                        },
                        new StringSetting()
                        {
                            Id = 2
                        }
                    },
                    Total = 4
                }));

            _mockSettingValueRepository.Setup(m => m.GetSettingValuesByIdsAsync(It.IsAny<IEnumerable<long>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((IEnumerable<SettingValue>)new List<SettingValue>()
                {
                    new SettingValue<string>()
                    {
                        SettingId = 2,
                        Value = "Abc"
                    }
                }));

            // Act
            var result = await _service.GetSettingValuePairsAsync(null, 0, 100);

            // Assert
            Assert.True(result.IsSuccessful);

            Assert.Equal(2, result.Value.Items.Count);
            Assert.Equal(4, result.Value.Total);
            Assert.Equal(0, result.Value.Skip);
            Assert.Equal(100, result.Value.Take);

            Assert.IsType<SettingValuePair<int>>(result.Value.Items[0]);
            Assert.Equal(1, result.Value.Items[0].GetSetting().Id);
            Assert.Equal(23, result.Value.Items[0].GetSettingValue().GetValue());

            Assert.IsType<SettingValuePair<string>>(result.Value.Items[1]);
            Assert.Equal(2, result.Value.Items[1].GetSetting().Id);
            Assert.Equal("Abc", result.Value.Items[1].GetSettingValue().GetValue());
        }

        #endregion
    }
}
