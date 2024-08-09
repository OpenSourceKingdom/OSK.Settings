using OSK.Settings.Abstractions.Validation;
using System.Collections.Generic;

namespace OSK.Settings.Abstractions.Settings
{
    public class StringSetting : Setting<string>
    {
        #region Variables

        public bool AllowNull { get; set; }

        public int? MaxCharacters { get; set; }

        public int? MinCharacters { get; set; }

        public bool AllowEmptyStrings { get; set; }

        public HashSet<string> AllowedValues { get; set; }

        #endregion

        #region Setting Overrides

        public override ValidationResult ValidateValue(string value)
        {
            if (value == null)
            {
                return AllowNull
                    ? ValidationResult.Successful
                    : new ValidationResult("The string value cannot be null.");
            }
            if (AllowedValues != null && AllowedValues.Count > 0)
            {
                return AllowedValues.Contains(value)
                    ? ValidationResult.Successful
                    : new ValidationResult($"The value, {value}, is not in the allowed set of values: {string.Join(", ", AllowedValues)}.");
            }
            if (MaxCharacters.HasValue && value.Length > MaxCharacters)
            {
                return new ValidationResult($"The value, {value}, exceeds the character limit of {MaxCharacters}.");
            }
            if (MinCharacters.HasValue && value.Length < MinCharacters)
            {
                return new ValidationResult($"The value, {value}, is below the character limit of {MinCharacters}.");
            }
            if (!AllowEmptyStrings && string.IsNullOrWhiteSpace(value))
            {
                return new ValidationResult($"Strings of only whitespace are not allowed.");
            }

            return ValidationResult.Successful;
        }

        public override ParameterValidationResult ValidateInternalParameters()
        {
            if (MaxCharacters.HasValue && MaxCharacters.Value <= 0)
            {
                return new ParameterValidationResult("Max characters can not be less than or equal to 0.");
            }
            if (MinCharacters.HasValue && MinCharacters.Value <= 0)
            {
                return new ParameterValidationResult("Min characters can not be less than or equal to 0.");
            }
            if (MinCharacters.HasValue && MaxCharacters.HasValue && MinCharacters > MaxCharacters)
            {
                return new ParameterValidationResult("Min characters can not be greater than the max characters allowed.");
            }

            return ParameterValidationResult.Successful;
        }

        #endregion
    }
}
