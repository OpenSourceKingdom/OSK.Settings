using OSK.Settings.Abstractions.Settings;
using OSK.Settings.Abstractions.Validation;
using Xunit;

namespace OSK.Settings.Abstractions.UnitTests.Settings
{
    public class IntegerSettingTests
    {
        #region ValidateValue

        [Theory]
        [InlineData(1f)]
        [InlineData(1d)]
        [InlineData("Thanks")]
        [InlineData(null)]
        public void ValidateValue_NonIntegerValue_Returns_Invalid(object value)
        {
            // Arrange
            var setting = new IntegerSetting();

            // Act
            var result = setting.ValidateValue(value);

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_IntegerLessThanMin_ReturnsInvalid()
        {
            // Arrange
            var setting = new IntegerSetting()
            {
                MinValue = 2
            };

            // Act
            var result = setting.ValidateValue(1);

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_IntegerGreaterThanMax_ReturnsInvalid()
        {
            // Arrange
            var setting = new IntegerSetting()
            {
                MaxValue = 876
            };

            // Act
            var result = setting.ValidateValue(877);

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_SpecificValuesAllowed_IntegerNotInList_ReturnsInvalid()
        {
            // Arrange
            var setting = new IntegerSetting()
            {
                AllowedValues = new HashSet<int>() { 44 }
            };

            // Act
            var result = setting.ValidateValue(3);

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_SpecificValuesAllowed_IntegerInList_ReturnsValid()
        {
            // Arrange
            var value = 1;
            var setting = new IntegerSetting()
            {
                AllowedValues = new HashSet<int>() { value }
            };

            // Act
            var result = setting.ValidateValue(value);

            // Assert
            Assert.Equal(ValidationResultType.Valid, result.ResultType);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void ValidateValue_IntegerAtOrBeforeMax_ReturnsValid(int amountBeforeMax)
        {
            // Arrange
            var value = 1;
            var setting = new IntegerSetting()
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
        public void ValidateValue_IntegerAtOrAfterMin_ReturnsValid(int amountAfterMin)
        {
            // Arrange
            var value = 5432;
            var setting = new IntegerSetting()
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
            var value = -44;
            var setting = new IntegerSetting()
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
            var value = -8888;
            var setting = new IntegerSetting()
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
            var value = -23;
            var setting = new IntegerSetting()
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
            var setting = new IntegerSetting();

            // Act
            var result = setting.ValidateInternalParameters();

            // Assert
            Assert.Equal(ParameterValidationResultType.Valid, result.ResultType);
        }

        #endregion

        #region GetDefaultSettingValuePair

        [Theory]
        [InlineData(555)]
        public void GetDefaultSettingValuePair_ReturnsSpecifiedSettingAndDefault(int value)
        {
            // Arrange
            var setting = new IntegerSetting()
            {
                Id = 117,
                DefaultValue = value
            };

            // Act
            var settingValuePair = setting.GetDefaultSettingValuePair();

            // Assert
            Assert.Equal(setting, settingValuePair.GetSetting());

            var settingValue = settingValuePair.GetSettingValue();
            Assert.IsType<SettingValue<int>>(settingValue);

            var actualValue = settingValue as SettingValue<int>;
            Assert.Equal(setting.Id, actualValue.SettingId);
            Assert.Equal(setting.DefaultValue, actualValue.Value);
        }

        #endregion
    }
}
