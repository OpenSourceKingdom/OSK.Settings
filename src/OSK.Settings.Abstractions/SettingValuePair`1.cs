using System;

namespace OSK.Settings.Abstractions
{
    public class SettingValuePair<T> : SettingValuePair
    {
        #region Variables

        public T Value { get; set; }

        public Setting Setting { get; set; }

        #endregion

        #region SettingValuePair Overrides

        public override Setting GetSetting() => Setting;

        public override SettingValue GetSettingValue() => new SettingValue<T>()
        {
            SettingId = Setting.Id,
            Value = Value
        };

        public override void SetValue(object value)
        {
            if (value is T typedValue)
            {
                Value = typedValue;
                return;
            }

            throw new InvalidOperationException($"The value, of type {value.GetType().FullName}, does not match the setting value type {typeof(T).FullName}.");
        }

        #endregion
    }
}
