using OSK.Settings.Abstractions.Validation;

namespace OSK.Settings.Abstractions
{
    public abstract class Setting<T> : Setting
    {
        #region Setting

        public T DefaultValue { get; set; }

        public abstract ValidationResult ValidateValue(T value);

        #endregion

        #region Setting Overrides

        public override SettingValuePair GetDefaultSettingValuePair()
        {
            return new SettingValuePair<T>()
            {
                Setting = this,
                Value = DefaultValue
            };
        }

        public override ValidationResult ValidateValue(object value)
        {
            if (value is T typedValue)
            {
                return ValidateValue(typedValue);
            }

            return new ValidationResult($"Unable to set a value of type, {value?.GetType()}, to a settng of type, {GetType().FullName}.");
        }
            
        #endregion
    }
}
