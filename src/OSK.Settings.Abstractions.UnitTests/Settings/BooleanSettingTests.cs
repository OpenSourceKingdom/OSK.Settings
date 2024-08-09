using OSK.Settings.Abstractions.Settings;
using OSK.Settings.Abstractions.Validation;
using Xunit;

namespace OSK.Settings.Abstractions.UnitTests.Settings
{
    public class BooleanSettingTests
    {
        #region ValidateValue

        [Theory]
        [InlineData(1)]
        [InlineData(1d)]
        [InlineData("Thanks")]
        [InlineData(null)]
        public void ValidateValue_NonBooleanValue_Returns_Invalid(object value)
        {
            // Arrange
            var setting = new BooleanSetting();

            // Act
            var result = setting.ValidateValue(value);

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ValidateValue_BooleanValue_ReturnsValid(bool value)
        {
            // Arrange
            var setting = new BooleanSetting();

            // Act
            var result = setting.ValidateValue(value);

            // Assert
            Assert.Equal(ValidationResultType.Valid, result.ResultType);
        }

        #endregion

        #region ValidateInternalParameters

        [Fact]
        public void ValidateInternalParameters_DefaultParameters_ReturnsValid()
        {
            // Arrange
            var setting = new BooleanSetting();

            // Act
            var result = setting.ValidateInternalParameters();
            
            // Assert
            Assert.Equal(ParameterValidationResultType.Valid, result.ResultType);
        }

        #endregion

        #region GetDefaultSettingValuePair

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetDefaultSettingValuePair_ReturnsSpecifiedSettingAndDefault(bool value)
        {
            // Arrange
            var setting = new BooleanSetting()
            {
                Id = 117,
                DefaultValue = value
            };

            // Act
            var settingValuePair = setting.GetDefaultSettingValuePair();

            // Assert
            Assert.Equal(setting, settingValuePair.GetSetting());

            var settingValue = settingValuePair.GetSettingValue();
            Assert.IsType<SettingValue<bool>>(settingValue);

            var actualValue = settingValue as SettingValue<bool>;
            Assert.Equal(setting.Id, actualValue.SettingId);
            Assert.Equal(setting.DefaultValue, actualValue.Value);
        }
             
        #endregion
    }
}
