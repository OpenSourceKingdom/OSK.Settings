using OSK.Functions.Outputs.Abstractions;
using OSK.Settings.Abstractions;
using OSK.Settings.Management.Ports;

namespace OSK.Settings.Management.Models
{
    public class ManagedSetting(SettingValuePair settingValuePair, ISettingsManager settingsManager)
    {
        #region Variables

        internal bool HasSetValue { get; set; }

        #endregion

        #region Helpers 

        public T ValueAs<T>() => (T)settingValuePair.GetSettingValue().GetValue();

        public IOutput SetValue(object value)
        {
            return settingsManager.StageSettingUpdate(this, value);
        }

        internal void ResetUpdate()
        {
            HasSetValue = false;
        }

        internal SettingValue GetSettingValue() => settingValuePair.GetSettingValue();

        internal Setting GetSetting() => settingValuePair.GetSetting();

        #endregion
    }
}
