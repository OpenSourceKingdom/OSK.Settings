using System;

namespace OSK.Settings.Abstractions
{
    public readonly struct SettingCategory: IEquatable<SettingCategory>
    {
        #region Variables

        public string Name { get; }

        #endregion

        #region Constructors 

        public SettingCategory(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
        }

        #endregion

        #region IEquatable

        public bool Equals(SettingCategory other)
        {
            return string.Equals(Name, other.Name, StringComparison.Ordinal);
        }

        #endregion

        #region Object Overrides

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return $"Category: {Name}";
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        #endregion

        #region Operators

        public static bool operator ==(SettingCategory a, SettingCategory b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SettingCategory a, SettingCategory b)
        {
            return !(a == b);
        }

        #endregion
    }
}
