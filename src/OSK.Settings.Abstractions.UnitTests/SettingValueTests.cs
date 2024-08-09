using OSK.Settings.Abstractions.Settings;
using Xunit;

namespace OSK.Settings.Abstractions.UnitTests
{
    public class SettingValueTests
    {
        #region GetValue

        [Fact]
        public void GetValue_ReturnsSetValue()
        {
            // Arrange
            var settingValue = new SettingValue<bool>() { Value = true };

            // Act
            var value = settingValue.GetValue();

            // Assert
            Assert.IsType<bool>(value);
            Assert.Equal(true, value);
        }

        #endregion

        #region ToSettingValuePair

        [Fact]
        public void ToSettingValuePair_NullSetting_ThrowsArgumentNullException()
        {
            // Arrange
            var settingValue = new SettingValue<bool>();

            // Act/Assert
            Assert.Throws<ArgumentNullException>(() => settingValue.ToSettingValuePair(null));
        }

        [Fact]
        public void ToSettingValuePair_SettingTypeDoesNotMatchSettingValueType_ThrowsInvalidOperationException()
        {
            // Arrange
            var settingValue = new SettingValue<bool>();

            // Act/Assert
            Assert.Throws<InvalidOperationException>(() => settingValue.ToSettingValuePair(new DateTimeSetting()));
        }

        [Fact]
        public void ToSettingValuePair_SettingValueIdDoesNotMachSettingId_ThrowsInvalidOperationException()
        {
            // Arrange
            var settingValue = new SettingValue<bool>()
            {
                SettingId = 432
            };
            var setting = new BooleanSetting()
            {
                Id = 1
            };

            // Act/Assert
            Assert.Throws<InvalidOperationException>(() => settingValue.ToSettingValuePair(setting));
        }

        [Fact]
        public void ToSettingValuePair_ValidSetting_ReturnsExpectedSettingValuePair()
        {
            // Arrange
            var settingValue = new SettingValue<bool>()
            {
                SettingId = 113
            };
            var setting = new BooleanSetting()
            {
                Id = 113
            };

            // Act
            var settingValuePair = settingValue.ToSettingValuePair(setting);

            // Assert
            Assert.NotNull(settingValuePair);
            Assert.Equal(setting, settingValuePair.GetSetting());
            Assert.Equal(settingValue.SettingId, settingValuePair.GetSettingValue().SettingId);
            Assert.Equal(settingValue.GetValue(), settingValuePair.GetSettingValue().GetValue());
        }

        #endregion
    }
}
