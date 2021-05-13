using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Graphics.Win2D
{
    public class W2DFontFamily : IFontFamily, IComparable<W2DFontFamily>
    {
        private readonly string _name;
        private readonly List<W2DFontStyle> _styleList = new List<W2DFontStyle>();
        private IFontStyle[] _fontStyles;

        public W2DFontFamily(string name)
        {
            _name = name;
        }

        public string Name => _name;

        public IFontStyle[] GetFontStyles()
        {
            return _fontStyles ?? (_fontStyles = InitializeFontStyles());
        }

        internal void AddStyle(W2DFontStyle style)
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
            if (obj.GetType() != typeof(W2DFontFamily))
                return false;
            W2DFontFamily other = (W2DFontFamily)obj;
            return _name == other._name;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_name != null ? _name.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(W2DFontFamily other)
        {
            return String.Compare(_name, other._name, StringComparison.Ordinal);
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

