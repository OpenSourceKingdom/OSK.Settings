using System.Collections.Generic;

namespace OSK.Extensions.Settings.Local.Models
{
    public class LocalSettings
    {
        public IEnumerable<LocalSettingValue> Settings { get; set; }
    }
}
