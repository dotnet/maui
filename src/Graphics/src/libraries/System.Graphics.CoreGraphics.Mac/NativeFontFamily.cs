using System.Collections.Generic;
using AppKit;
using Foundation;

namespace System.Graphics.CoreGraphics
{
    public class NativeFontFamily : IFontFamily, IComparable<IFontFamily>, IComparable
    {
        private readonly string _name;
        private IFontStyle[] _fontStyles;

        public NativeFontFamily(string name)
        {
            _name = name;
        }

        public string Name => _name;

        public IFontStyle[] GetFontStyles()
        {
            return _fontStyles ?? (_fontStyles = InitializeFontStyles());
        }

        private IFontStyle[] InitializeFontStyles()
        {
            var traits = NSFontManager.SharedFontManager.AvailableMembersOfFontFamily(_name);

            var styles = new List<IFontStyle>();

            if (traits != null)
            {
                int index = 0;
                Array.ForEach(
                    traits,
                    subarray =>
                    {
                        var array = (NSArray) ObjCRuntime.Runtime.GetNSObject(subarray.Handle);
                        var id = (NSString) ObjCRuntime.Runtime.GetNSObject(array.ValueAt(0));
                        var name = (NSString) ObjCRuntime.Runtime.GetNSObject(array.ValueAt(1));
                        var weight = FontUtils.GetFontWeight(name);
                        var styleType = FontUtils.GetStyleType(name);

                        string fullName = _name;
                        if (index > 0)
                            fullName = $"{_name} {name}";

                        styles.Add(new NativeFontStyle(this, id, name, fullName, weight, styleType));
                        index++;
                    });
            }

            styles.Sort();
            return styles.ToArray();
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
                return (_name != null ? _name.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(IFontFamily other)
        {
            return string.Compare(_name, other?.Name, StringComparison.Ordinal);
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as IFontFamily);
        }
    }
}