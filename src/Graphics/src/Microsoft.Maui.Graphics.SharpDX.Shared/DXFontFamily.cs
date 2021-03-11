using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Graphics.SharpDX
{
    public class DXFontFamily : IFontFamily, IComparable<DXFontFamily>
    {
        private readonly string _name;
        private readonly List<DXFontStyle> _styleList = new List<DXFontStyle>();
        private IFontStyle[] _fontStyles;

        public DXFontFamily(string name)
        {
            _name = name;
        }

        public string Name => _name;

        public IFontStyle[] GetFontStyles()
        {
            if (_fontStyles == null)
                _fontStyles = InitializeFontStyles();

            return _fontStyles;
        }

        internal void AddStyle(DXFontStyle style)
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
            return _styleList.OfType<IFontStyle>().ToArray();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(DXFontFamily))
                return false;
            DXFontFamily other = (DXFontFamily) obj;
            return _name == other._name;
        }

        public override int GetHashCode()
        {
            return (_name != null ? _name.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(DXFontFamily other)
        {
            return _name.CompareTo(other._name);
        }

        public void RemoveDuplicates()
        {
            _styleList.Sort();

            var ids = new List<string>();
            for (int i = 0; i < _styleList.Count; i++)
            {
                var style = _styleList[i];
                if (ids.Contains(style.Id))
                {
                    _styleList.RemoveAt(i);
                    i--;
                }
                else
                {
                    ids.Add(style.Id);
                }
            }
        }
    }
}