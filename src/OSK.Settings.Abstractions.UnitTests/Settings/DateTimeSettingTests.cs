using OSK.Settings.Abstractions.Settings;
using OSK.Settings.Abstractions.Validation;
using Xunit;

namespace OSK.Settings.Abstractions.UnitTests.Settings
{
    public class DateTimeSettingTests
    {
        #region ValidateValue

        [Theory]
        [InlineData(1)]
        [InlineData(1d)]
        [InlineData("Thanks")]
        [InlineData(null)]
        public void ValidateValue_NonDateTimeValue_Returns_Invalid(object value)
        {
            // Arrange
            var setting = new DateTimeSetting();

            // Act
            var result = setting.ValidateValue(value);

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_DateTimeLessThanMin_ReturnsInvalid()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var setting = new DateTimeSetting()
            {
                MinValue = now
            };

            // Act
            var result = setting.ValidateValue(now.Subtract(TimeSpan.FromDays(1)));

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_DateTimeGreaterThanMax_ReturnsInvalid()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var setting = new DateTimeSetting()
            {
                MaxValue = now
            };

            // Act
            var result = setting.ValidateValue(now.AddDays(1));

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_SpecificValuesAllowed_DateTimeNotInList_ReturnsInvalid()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var setting = new DateTimeSetting()
            {
                AllowedValues = new HashSet<DateTime>() { now }
            };

            // Act
            var result = setting.ValidateValue(now.AddHours(1));

            // Assert
            Assert.Equal(ValidationResultType.Invalid, result.ResultType);
        }

        [Fact]
        public void ValidateValue_SpecificValuesAllowed_DateTimeInList_ReturnsValid()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var setting = new DateTimeSetting()
            {
                AllowedValues = new HashSet<DateTime>() { now }
            };

            // Act
            var result = setting.ValidateValue(now);

            // Assert
            Assert.Equal(ValidationResultType.Valid, result.ResultType);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void ValidateValue_DateTimeAtOrBeforeMax_ReturnsValid(int daysBeforeMax)
        {
            // Arrange
            var now = DateTime.UtcNow;
            var setting = new DateTimeSetting()
            {
                MaxValue = now
            };

            // Act
            var result = setting.ValidateValue(now.Subtract(TimeSpan.FromDays(daysBeforeMax)));

            // Assert
            Assert.Equal(ValidationResultType.Valid, result.ResultType);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void ValidateValue_DateTimeAtOrAfterMin_ReturnsValid(int daysAfterMin)
        {
            // Arrange
            var now = DateTime.UtcNow;
            var setting = new DateTimeSetting()
            {
                MinValue = now
            };

            // Act
            var result = setting.ValidateValue(now.AddDays(daysAfterMin));

            // Assert
            Assert.Equal(ValidationResultType.Valid, result.ResultType);
        }

        #endregion

        #region ValidateInternalParameters

        [Fact]
        public void ValidateInternalParameters_MinGreaterThanMax_ReturnsInvalid()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var setting = new DateTimeSetting()
            {
                MaxValue = now,
                MinValue = now.AddHours(1)
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
            var now = DateTime.UtcNow;
            var setting = new DateTimeSetting()
            {
                MaxValue = now.Subtract(TimeSpan.FromHours(1)),
                MinValue = now
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
            var now = DateTime.UtcNow;
            var setting = new DateTimeSetting()
            {
                MaxValue = now,
                MinValue = now
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
            var setting = new DateTimeSetting();

            // Act
            var result = setting.ValidateInternalParameters();

            // Assert
            Assert.Equal(ParameterValidationResultType.Valid, result.ResultType);
        }

        #endregion

        #region GetDefaultSettingValuePair

        [Theory]
        [InlineData("2024-03-01")]
        public void GetDefaultSettingValuePair_ReturnsSpecifiedSettingAndDefault(string value)
        {
            // Arrange
            var date = DateTime.Parse(value);
            var setting = new DateTimeSetting()
            {
                Id = 117,
                DefaultValue = date
            };

            // Act
            var settingValuePair = setting.GetDefaultSettingValuePair();

            // Assert
            Assert.Equal(setting, settingValuePair.GetSetting());

            var settingValue = settingValuePair.GetSettingValue();
            Assert.IsType<SettingValue<DateTime>>(settingValue);

            var actualValue = settingValue as SettingValue<DateTime>;
            Assert.Equal(setting.Id, actualValue.SettingId);
            Assert.Equal(setting.DefaultValue, actualValue.Value);
        }

        #endregion
    }
}
