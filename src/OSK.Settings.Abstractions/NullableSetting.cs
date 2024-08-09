using OSK.Settings.Abstractions.Validation;

namespace OSK.Settings.Abstractions
{
    public abstract class NullableSetting<T> : Setting<T?>
        where T : struct
    {
        #region Variables

        public bool AllowNull { get; set; }

        #endregion

        #region Setting Overrides

        public override ValidationResult ValidateValue(T? value)
        {
            if (value == null)
            {
                return AllowNull
                    ? ValidationResult.Successful
                    : new ValidationResult($"An attempt was made to set a null value to a setting, {GetType().FullName}, which does not allow null.");
            }

            return ValidateValue(value.Value);
        }

        #endregion

        #region Helpers

        protected abstract ValidationResult ValidateValue(T value);

        #endregion
    }
}
