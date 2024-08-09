using OSK.Settings.Abstractions.Validation;

namespace OSK.Settings.Abstractions.Settings
{
    public class BooleanSetting : Setting<bool>
    {
        #region Setting Overrides

        public override ValidationResult ValidateValue(bool value)
        {
            return ValidationResult.Successful;
        }

        public override ParameterValidationResult ValidateInternalParameters()
        {
            return ParameterValidationResult.Successful;
        }

        #endregion
    }
}
