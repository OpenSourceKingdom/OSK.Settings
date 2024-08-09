using Moq;
using OSK.Extensions.Settings.Providers.Internal.Services;
using OSK.Extensions.Settings.Providers.Ports;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Functions.Outputs.Mocks;
using OSK.Settings.Abstractions;
using OSK.Settings.Abstractions.Settings;
using OSK.Settings.Ports;
using System.Net;
using Xunit;

namespace OSK.Extensions.Settings.Providers.UnitTests.Internal.Services
{
    public class DefaultSettingsRepositoryTests
    {
        #region Variables

        private readonly Mock<ISettingsProvider> _provider;
        private readonly IOutputFactory<DefaultSettingsRepository> _outputFactory;
           
        private readonly DefaultSettingsRepository _repository;

        #endregion

        #region Constructors

        public DefaultSettingsRepositoryTests()
        {
            _provider = new Mock<ISettingsProvider>();
            _outputFactory = new MockOutputFactory<DefaultSettingsRepository>();

            _repository = new DefaultSettingsRepository(new List<ISettingsProvider>() { _provider.Object },
                _outputFactory);
        }

        #endregion

        #region InitializeAsync

        [Fact]
        public async Task InitializeAsync_CallToProviderReturnsError_ReturnsError()
        {
            // Arrange
            _provider.Setup(m => m.GetSettingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.BadRequest<IEnumerable<Setting>>("A bad day"));

            // Act
            var result = await _repository.InitializeAsync(CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.BadRequest, result.Code.StatusCode);
        }

        [Fact]
        public async Task InitializeAsync_ValidCallToProvider_ReturnsSuccessfullly()
        {
            // Arrange
            _provider.Setup(m => m.GetSettingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((IEnumerable<Setting>)new List<Setting>()));

            // Act
            var result = await _repository.InitializeAsync(CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccessful);
        }

        #endregion

        #region GetAsync

        [Fact]
        public async Task GetAsync_SettingDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _provider.Setup(m => m.GetSettingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((IEnumerable<Setting>)new List<Setting>()));

            // Act
            var result = await _repository.GetAsync(1);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Equal(HttpStatusCode.NotFound, result.Code.StatusCode);
        }

        [Fact]
        public async Task GetAsync_ValidSetting_ReturnsSuccessflly()
        {
            // Arrange
            var setting = new BooleanSetting()
            {
                Id = 7
            };
            _provider.Setup(m => m.GetSettingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((IEnumerable<Setting>)new List<Setting>()
                {
                    setting
                }));

            // Act
            var result = await _repository.GetAsync(setting.Id);

            // Assert
            Assert.True(result.IsSuccessful);
        }

        #endregion

        #region GetByNameAsync

        [Fact]
        public async Task GetByNameAsync_NoSettingsWithName_ReturnsEmptyList()
        {
            // Arrange
            _provider.Setup(m => m.GetSettingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((IEnumerable<Setting>)new List<Setting>()));

            // Act
            var result = await _repository.GetByNameAsync("Hello");

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetByNameAsync_SettingNameInUse_ReturnsSettingsSuccessfully()
        {
            // Arrange
            var setting = new BooleanSetting()
            {
                Id = 7,
                Name = "Hello"
            };
            _provider.Setup(m => m.GetSettingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((IEnumerable<Setting>)new List<Setting>()
                {
                    setting
                }));

            // Act
            var result = await _repository.GetByNameAsync(setting.Name);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Single(result.Value);
            Assert.Equal(setting, result.Value.First());
        }

        #endregion

        #region GetPageAsync

        [Fact]
        public async Task GetPageAsync_NullCategory_ReturnsListWithAllCategories()
        {
            // Arrange
            var settingCategory = new SettingCategory("H");
            var setting = new BooleanSetting()
            {
                Id = 1,
                Name = "Hello",
                Category = settingCategory
            };
            var setting1 = new BooleanSetting()
            {
                Id = 2,
                Name = "Hello World",
                Category = settingCategory
            };

            var settingCategory1 = new SettingCategory("Abc");
            var setting2 = new IntegerSetting()
            {
                Id = 3,
                Name = "Hello Integer",
                Category = settingCategory1
            };

            var setting3 = new DateTimeSetting()
            {
                Id = 4,
                Name = "Bloop",
                Category = null
            };
            var setting4 = new DateTimeSetting()
            {
                Id = 5,
                Name = "Bloop2",
                Category = null
            };

            _provider.Setup(m => m.GetSettingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((IEnumerable<Setting>)new List<Setting>()
                {
                    setting,
                    setting1, 
                    setting2, 
                    setting3,
                    setting4
                }));

            // Act
            var result = await _repository.GetPageAsync(null, 1, 3);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Equal(3, result.Value.Items.Count);
            Assert.Equal(5, result.Value.Total);
            Assert.Equal(1, result.Value.Skip);
            Assert.Equal(3, result.Value.Take);

            Assert.Equal(setting1, result.Value.Items[0]);
            Assert.Equal(setting2, result.Value.Items[1]);
            Assert.Equal(setting3, result.Value.Items[2]);
        }

        [Fact]
        public async Task GetPageAsync_SpecificCategory_ReturnsListWithSpecificCategory()
        {
            // Arrange
            var settingCategory = new SettingCategory("H");
            var setting = new BooleanSetting()
            {
                Id = 1,
                Name = "Hello",
                Category = settingCategory
            };
            var setting1 = new BooleanSetting()
            {
                Id = 2,
                Name = "Hello World",
                Category = settingCategory
            };

            var settingCategory1 = new SettingCategory("Abc");
            var setting2 = new IntegerSetting()
            {
                Id = 3,
                Name = "Hello Integer",
                Category = settingCategory1
            };

            var setting3 = new DateTimeSetting()
            {
                Id = 4,
                Name = "Bloop",
                Category = null
            };
            var setting4 = new DateTimeSetting()
            {
                Id = 5,
                Name = "Bloop2",
                Category = null
            };

            _provider.Setup(m => m.GetSettingsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_outputFactory.Success((IEnumerable<Setting>)new List<Setting>()
                {
                    setting,
                    setting1,
                    setting2,
                    setting3,
                    setting4
                }));

            // Act
            var result = await _repository.GetPageAsync(settingCategory1, 0, 3);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Single(result.Value.Items);
            Assert.Equal(5, result.Value.Total);
            Assert.Equal(0, result.Value.Skip);
            Assert.Equal(3, result.Value.Take);

            Assert.Equal(setting2, result.Value.Items[0]);
        }

        #endregion
    }
}
