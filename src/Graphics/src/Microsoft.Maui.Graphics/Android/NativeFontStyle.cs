using System;
using System.IO;

namespace Microsoft.Maui.Graphics.Android
{
    public class NativeFontStyle : IFontStyle, IComparable
    {
        private readonly NativeFontFamily _family;

        public NativeFontStyle(NativeFontFamily family, string id, string name, string fullName, int weight, FontStyleType styleType, string path, bool resource = false)
        {
            _family = family;
            Id = id;
            Name = name;
            FullName = fullName;
            Weight = weight;
            StyleType = styleType;
            Path = path;
            Resource = resource;
        }

        public IFontFamily FontFamily => _family;

        public string Id { get; }

        public string Name { get; }

        public string FullName { get; }

        public int Weight { get; }

        public FontStyleType StyleType { get; }

        public string Path { get; }

        public bool Resource { get; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(NativeFontStyle))
                return false;
            NativeFontStyle other = (NativeFontStyle) obj;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Id != null ? Id.GetHashCode() : 0;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(IFontStyle other)
        {
            if (other == null)
                return -1;

            if (Name.Equals("Regular") || Name.Equals("Plain"))
            {
                return -1;
            }
            else if (other.Name.Equals("Regular") || other.Name.Equals("Plain"))
            {
                return 1;
            }

            return String.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        public Stream OpenStream()
        {
            return null;
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as IFontStyle);
        }
    }
}