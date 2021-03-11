using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Graphics.Android
{
    public class NativeFontFamily : IFontFamily, IComparable<IFontFamily>, IComparable
    {
        private readonly string _name;
        private IFontStyle[] _fontStyles;
        private readonly List<IFontStyle> _styleList = new List<IFontStyle>();

        public NativeFontFamily(string name)
        {
            _name = name;
        }

        public string Name => _name;

        public IFontStyle[] GetFontStyles()
        {
            return _fontStyles ?? (_fontStyles = InitializeFontStyles());
        }

        internal void AddStyle(NativeFontStyle style)
        {
            _fontStyles = null;
            _styleList.Add(style);
        }

        internal bool HasStyle(string style)
        {
            return _styleList.Exists(s => s.Name.Equals(style));
        }

        private IFontStyle[] InitializeFontStyles()
        {
            _styleList.Sort();
            return _styleList.ToArray();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(NativeFontFamily))
                return false;
            NativeFontFamily other = (NativeFontFamily) obj;
            return _name == other._name;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return _name != null ? _name.GetHashCode() : 0;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(IFontFamily other)
        {
            return String.Compare(_name, other.Name, StringComparison.Ordinal);
        }

        public int CompareTo(object obj)
        {
            if (obj is IFontFamily other)
                return CompareTo(other);

            return -1;
        }
    }
}