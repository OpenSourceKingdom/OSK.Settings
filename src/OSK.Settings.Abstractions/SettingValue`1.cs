using System;

namespace OSK.Settings.Abstractions
{
    public class SettingValue<T> : SettingValue
    {
        #region Variables

        public T Value { get; set; }

        #endregion

        #region SettingValue overrides

        public override object GetValue() => Value;

        public override SettingValuePair ToSettingValuePair(Setting setting)
        {
            if (setting == null)
            {
                throw new ArgumentNullException(nameof(setting));
            }
            if (SettingId != setting.Id)
            {
                throw new InvalidOperationException($"The setting id {setting.Id} does not match the setting value id {SettingId}.");
            }
            if (setting is Setting<T> _)
            {
                return new SettingValuePair<T>()
                {
                    Setting = setting,
                    Value = Value
                };
            }

            throw new InvalidOperationException($"The setting {setting.GetType().FullName} is not capable of being paired with a setting of type {typeof(T).FullName}.");
        }

        #endregion
    }
}
