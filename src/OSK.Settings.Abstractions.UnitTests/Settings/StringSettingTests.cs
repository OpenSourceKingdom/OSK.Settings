using OSK.Settings.Abstractions.Settings;
using OSK.Settings.Abstractions.Validation;
using System.Text;
using Xunit;

namespace OSK.Settings.Abstractions.UnitTests.Settings
{
    public class StringSettingTests
    {
        #region ValidateValue

        [Theory]
        [InlineData(1f)]
        [InlineData(1d)]
        public void ValidateValue_NonStringValue_Returns_Invalid(object value)
        {
            // Arrange
            var setting = new StringSetting();

            // Act
            var result = setting.ValidateValue(value);

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_NullsNotAllowed_Null_Returns_Invalid()
        {
            // Arrange
            var setting = new StringSetting()
            {
                AllowNull = false
            };

            // Act
            var result = setting.ValidateValue(null);

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_CharactersLessThanMin_ReturnsInvalid()
        {
            // Arrange
            var setting = new StringSetting()
            {
                MinCharacters = 2
            };

            // Act
            var result = setting.ValidateValue("n");

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_CharactersGreaterThanMax_ReturnsInvalid()
        {
            // Arrange
            var setting = new StringSetting()
            {
                MaxCharacters = 3
            };

            // Act
            var result = setting.ValidateValue("abcd");

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_SpecificValuesAllowed_StringNotInList_ReturnsInvalid()
        {
            // Arrange
            var setting = new StringSetting()
            {
                AllowedValues = new HashSet<string>() { "hi" }
            };

            // Act
            var result = setting.ValidateValue("hey");

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("         ")]
        public void ValidateValue_BlankStringsNotAllowed_StringIsWhitespace_ReturnsInvalid(string str)
        {
            // Arrange
            var setting = new StringSetting()
            {
                AllowEmptyStrings = false
            };

            // Act
            var result = setting.ValidateValue(str);

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_NullsAllowed_Null_ReturnsValid()
        {
            // Arrange
            var setting = new StringSetting()
            {
                AllowNull = true
            };

            // Act
            var result = setting.ValidateValue(null);

            // Assert
            Assert.Equal(ValidationResultType.Valid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_SpecificValuesAllowed_StringInList_ReturnsValid()
        {
            // Arrange
            var value = "abc";
            var setting = new StringSetting()
            {
                AllowedValues = new HashSet<string>() { value }
            };

            // Act
            var result = setting.ValidateValue(value);

            // Assert
            Assert.Equal(ValidationResultType.Valid, result.ResultType);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void ValidateValue_CharctersAtOrBeforeMax_ReturnsValid(int amountBeforeMax)
        {
            // Arrange
            var value = 2;
            var setting = new StringSetting()
            {
                MaxCharacters = value
            };

            var strBuilder = new StringBuilder();
            for (var i = 0; i < value - amountBeforeMax; i++)
            {
                strBuilder.Append("a");
            }

            // Act
            var result = setting.ValidateValue(strBuilder.ToString());

            // Assert
            Assert.Equal(ValidationResultType.Valid, result.ResultType);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void ValidateValue_CharactersAtOrAfterMin_ReturnsValid(int amountAfterMin)
        {
            // Arrange
            var value = 21;
            var setting = new StringSetting()
            {
                MinCharacters = value
            };

            var strBuilder = new StringBuilder();
            for (var i = 0; i < value + amountAfterMin; i++)
            {
                strBuilder.Append("a");
            }

            // Act
            var result = setting.ValidateValue(strBuilder.ToString());

            // Assert
            Assert.Equal(ValidationResultType.Valid, result.ResultType);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("         ")]
        public void ValidateValue_BlankStringsAllowed_StringIsWhitespace_ReturnsValid(string str)
        {
            // Arrange
            var setting = new StringSetting()
            {
                AllowEmptyStrings = true
            };

            // Act
            var result = setting.ValidateValue(str);

            // Assert
            Assert.Equal(ValidationResultType.Valid, result.ResultType);
        }

        #endregion

        #region ValidateInternalParameters

        [Fact]
        public void ValidateInternalParameters_MinCharactersLessThan0_ReturnsInvalid()
        {
            // Arrange
            var setting = new StringSetting()
            {
                MinCharacters = -1
            };

            // Act
            var result = setting.ValidateInternalParameters();

            // Assert
            Assert.Equal(ParameterValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateInternalParameters_MaxCharactersLessThan0_ReturnsInvalid()
        {
            // Arrange
            var setting = new StringSetting()
            {
                MaxCharacters = -1
            };

            // Act
            var result = setting.ValidateInternalParameters();

            // Assert
            Assert.Equal(ParameterValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateInternalParameters_MinCharactersGreaterThanMaxCharacters_ReturnsInvalid()
        {
            // Arrange
            var value = 1;
            var setting = new StringSetting()
            {
                MaxCharacters = value,
                MinCharacters = value + 1
            };

            // Act
            var result = setting.ValidateInternalParameters();

            // Assert
            Assert.Equal(ParameterValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateInternalParameters_MaxCharactersLessThanMinCharacters_ReturnsInvalid()
        {
            // Arrange
            var value = 5;
            var setting = new StringSetting()
            {
                MaxCharacters = value - 1,
                MinCharacters = value
            };

            // Act
            var result = setting.ValidateInternalParameters();

            // Assert
            Assert.Equal(ParameterValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateInternalParameters_MaxCharactersEqualsMinCharacters_ReturnsValid()
        {
            // Arrange
            var value = 55;
            var setting = new StringSetting()
            {
                MaxCharacters = value,
                MinCharacters = value
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
            var setting = new StringSetting();

            // Act
            var result = setting.ValidateInternalParameters();

            // Assert
            Assert.Equal(ParameterValidationResultType.Valid, result.ResultType);
        }

        #endregion

        #region GetDefaultSettingValuePair

        [Theory]
        [InlineData("hello world")]
        public void GetDefaultSettingValuePair_ReturnsSpecifiedSettingAndDefault(string value)
        {
            // Arrange
            var setting = new StringSetting()
            {
                Id = 117,
                DefaultValue = value
            };

            // Act
            var settingValuePair = setting.GetDefaultSettingValuePair();

            // Assert
            Assert.Equal(setting, settingValuePair.GetSetting());

            var settingValue = settingValuePair.GetSettingValue();
            Assert.IsType<SettingValue<string>>(settingValue);

            var actualValue = settingValue as SettingValue<string>;
            Assert.Equal(setting.Id, actualValue.SettingId);
            Assert.Equal(setting.DefaultValue, actualValue.Value);
        }

        #endregion
    }
}
