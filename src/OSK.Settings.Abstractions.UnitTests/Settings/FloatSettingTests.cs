using OSK.Settings.Abstractions.Settings;
using OSK.Settings.Abstractions.Validation;
using Xunit;

namespace OSK.Settings.Abstractions.UnitTests.Settings
{
    public class FloatSettingTests
    {
        #region ValidateValue

        [Theory]
        [InlineData(1)]
        [InlineData(1d)]
        [InlineData("Thanks")]
        [InlineData(null)]
        public void ValidateValue_NonFloatValue_Returns_Invalid(object value)
        {
            // Arrange
            var setting = new FloatSetting();

            // Act
            var result = setting.ValidateValue(value);

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_FloatLessThanMin_ReturnsInvalid()
        {
            // Arrange
            var setting = new FloatSetting()
            {
                MinValue = 1.5f
            };

            // Act
            var result = setting.ValidateValue(1.4f);

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_FloatGreaterThanMax_ReturnsInvalid()
        {
            // Arrange
            var setting = new FloatSetting()
            {
                MaxValue = 2.14f
            };

            // Act
            var result = setting.ValidateValue(2.15f);

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_SpecificValuesAllowed_FloatNotInList_ReturnsInvalid()
        {
            // Arrange
            var setting = new FloatSetting()
            {
                AllowedValues = new HashSet<float>() { 4.2f }
            };

            // Act
            var result = setting.ValidateValue(3f);

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_SpecificValuesAllowed_FloatInList_ReturnsValid()
        {
            // Arrange
            var value = 1.11f;
            var setting = new FloatSetting()
            {
                AllowedValues = new HashSet<float>() { value }
            };

            // Act
            var result = setting.ValidateValue(value);

            // Assert
            Assert.Equal(ValidationResultType.Valid, result.ResultType);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void ValidateValue_FloatAtOrBeforeMax_ReturnsValid(int amountBeforeMax)
        {
            // Arrange
            var value = 1.5f;
            var setting = new FloatSetting()
            {
                MaxValue = value
            };

            // Act
            var result = setting.ValidateValue(value - amountBeforeMax);

            // Assert
            Assert.Equal(ValidationResultType.Valid, result.ResultType);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void ValidateValue_FloatAtOrAfterMin_ReturnsValid(int amountAfterMin)
        {
            // Arrange
            var value = 32.8f;
            var setting = new FloatSetting()
            {
                MinValue = value
            };

            // Act
            var result = setting.ValidateValue(value + amountAfterMin);

            // Assert
            Assert.Equal(ValidationResultType.Valid, result.ResultType);
        }

        #endregion

        #region ValidateInternalParameters

        [Fact]
        public void ValidateInternalParameters_MinGreaterThanMax_ReturnsInvalid()
        {
            // Arrange
            var value = 8.88f;
            var setting = new FloatSetting()
            {
                MaxValue = value,
                MinValue = value + 1
            };

            // Act
            var result = setting.ValidateInternalParameters();

            // Assert
            Assert.Equal(ParameterValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateInternalParameters_MaxLessThanMin_ReturnsInvalid()
        {
            // Arrange
            var value = 6.77f;
            var setting = new FloatSetting()
            {
                MaxValue = value - 1,
                MinValue = value
            };

            // Act
            var result = setting.ValidateInternalParameters();

            // Assert
            Assert.Equal(ParameterValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateInternalParameters_MaxEqualsMin_ReturnsValid()
        {
            // Arrange
            var value = 9999;
            var setting = new FloatSetting()
            {
                MaxValue = value,
                MinValue = value
            };

            // Act
            var result = setting.ValidateInternalParameters();

            // Assert
            Assert.Equal(ParameterValidationResultType.Valid, result.ResultType);
        }

        [Fact]
        public void ValidateInternalParameters_DefaultParameters_ReturnsValid()
        {
            // Arrange
            var setting = new FloatSetting();

            // Act
            var result = setting.ValidateInternalParameters();

            // Assert
            Assert.Equal(ParameterValidationResultType.Valid, result.ResultType);
        }

        #endregion

        #region GetDefaultSettingValuePair

        [Theory]
        [InlineData(33)]
        public void GetDefaultSettingValuePair_ReturnsSpecifiedSettingAndDefault(float value)
        {
            // Arrange
            var setting = new FloatSetting()
            {
                Id = 117,
                DefaultValue = value
            };

            // Act
            var settingValuePair = setting.GetDefaultSettingValuePair();

            // Assert
            Assert.Equal(setting, settingValuePair.GetSetting());

            var settingValue = settingValuePair.GetSettingValue();
            Assert.IsType<SettingValue<float>>(settingValue);

            var actualValue = settingValue as SettingValue<float>;
            Assert.Equal(setting.Id, actualValue.SettingId);
            Assert.Equal(setting.DefaultValue, actualValue.Value);
        }

        #endregion
    }
}
