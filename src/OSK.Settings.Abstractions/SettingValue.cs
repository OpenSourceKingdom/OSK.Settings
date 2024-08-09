namespace OSK.Settings.Abstractions
{
    public abstract class SettingValue
    {
        #region Vaiables

        public long SettingId { get; set; }

        #endregion

        #region Helpers

        public abstract object GetValue();

        public abstract SettingValuePair ToSettingValuePair(Setting setting);

        #endregion
    }
}
