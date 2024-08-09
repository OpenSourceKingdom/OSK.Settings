using OSK.Settings.Abstractions.Settings;
using Xunit;

namespace OSK.Settings.Abstractions.UnitTests
{
    public class SettingValuePairTests
    {
        #region GetSetting

        [Fact]
        public void GetSetting_ReturnsExpectedSetting()
        {
            // Arrange
            var setting = new DateTimeSetting();
            var settingValuePair = new SettingValuePair<DateTime>() 
            { 
                Setting = setting
            };

            // Act
            var actualSetting = settingValuePair.GetSetting();

            // Assert
            Assert.Equal(setting, actualSetting);
        }

        #endregion

        #region SettingValue

        [Fact]
        public void SettingValue_ReturnsExpectedSettingValue()
        {
            // Arrange
            var setting = new DateTimeSetting()
            {
                Id = 83214
            };
            var settingValuePair = new SettingValuePair<DateTime>()
            {
                Setting = setting,
                Value = DateTime.UtcNow
            };

            // Act
            var actualSettingValue = settingValuePair.GetSettingValue();

            // Assert
            Assert.Equal(setting.Id, actualSettingValue.SettingId);
            Assert.Equal(settingValuePair.Value, actualSettingValue.GetValue());
        }

        #endregion
    }
}
