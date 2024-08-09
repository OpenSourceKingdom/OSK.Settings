using System.Collections.Generic;

namespace OSK.Settings.Abstractions
{
    public abstract class StructSetting<T> : Setting<T>
        where T : struct
    {
        #region Variables

        public T? MaxValue { get; set; }

        public T? MinValue { get; set; }

        public HashSet<T> AllowedValues { get; set; }

        #endregion
    }
}
