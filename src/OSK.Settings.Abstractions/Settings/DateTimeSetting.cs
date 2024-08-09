using OSK.Settings.Abstractions.Validation;
using System;

namespace OSK.Settings.Abstractions.Settings
{
    public class DateTimeSetting: StructSetting<DateTime>
    {
        #region Settings Overrides

        public override ValidationResult ValidateValue(DateTime value)
        {
            if (AllowedValues != null && AllowedValues.Count > 0)
            {
                return AllowedValues.Contains(value)
                ? ValidationResult.Successful
                    : new ValidationResult($"The value, {value}, is not in the allowed set of values: {string.Join(", ", AllowedValues)}.");
            }
            if (MaxValue.HasValue && value > MaxValue)
            {
                return new ValidationResult($"The value, {value}, exceeds the max of {MaxValue}.");
            }
            if (MinValue.HasValue && value < MinValue)
            {
                return new ValidationResult($"The value, {value}, is below the min of {MinValue}.");
            }

            return ValidationResult.Successful;
        }

        public override ParameterValidationResult ValidateInternalParameters()
        {
            if (MaxValue.HasValue && MinValue.HasValue && MinValue > MaxValue)
            {
                return new ParameterValidationResult($"{nameof(MinValue)} can not be greater than {nameof(MaxValue)}");
            }

            return ParameterValidationResult.Successful;
        }

        #endregion
    }
}
