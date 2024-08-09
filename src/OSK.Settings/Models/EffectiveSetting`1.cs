using OSK.Settings.Abstractions;

namespace OSK.Settings.Models
{
    public class EffectiveSetting<T>(Setting setting, T effectiveValue) : EffectiveSetting(setting.Id)
    {
        public Setting Setting => setting;

        public T Value => effectiveValue;
    }
}
