using OSK.Settings.Abstractions.Validation;

namespace OSK.Settings.Abstractions
{
    public abstract class Setting
    {
        #region ISetting

        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public SettingCategory? Category { get; set; }

        public bool IsRequired { get; set; }

        public bool IsReadOnly { get; set; }

        public abstract SettingValuePair GetDefaultSettingValuePair();

        public abstract ValidationResult ValidateValue(object value);

        public abstract ParameterValidationResult ValidateInternalParameters();

        #endregion
    }
}
