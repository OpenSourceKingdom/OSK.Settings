namespace OSK.Settings.Abstractions
{
    public abstract class SettingValuePair
    {
        public abstract Setting GetSetting();

        public abstract SettingValue GetSettingValue();

        public abstract void SetValue(object value);
    }
}
