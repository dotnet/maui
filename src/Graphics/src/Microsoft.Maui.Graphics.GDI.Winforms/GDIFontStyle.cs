using System;
using System.Drawing;
using System.IO;

namespace Microsoft.Maui.Graphics.GDI
{
    public class GDIFontStyle : IFontStyle
    {
        private readonly GDIFontFamily _family;

        public GDIFontStyle(GDIFontFamily family, string id, string name, string fullName, FontStyle style, int weight)
        {
            _family = family;
            Id = id;
            Name = name;
            FullName = fullName;
            StyleType = style.AsStyleType();
            Weight = weight;
        }

        public IFontFamily FontFamily => _family;

        public string Id { get; }

        public string Name { get; }

        public string FullName { get; }

        public FontStyleType StyleType { get; }

        public int Weight { get; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(GDIFontStyle))
                return false;
            GDIFontStyle other = (GDIFontStyle) obj;
            return Id == other.Id;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                return Id?.GetHashCode() ?? 0;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(IFontStyle other)
        {
            if (Name.Equals("Regular") || Name.Equals("Plain") || Name.Equals("Normal"))
            {
                return -1;
            }

            if (other.Name.Equals("Regular") || other.Name.Equals("Plain") || other.Name.Equals("Normal"))
            {
                return 1;
            }

            return String.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        public Stream OpenStream()
        {
            return null;
        }
    }

    public static class FontStyleExtensions
    {
        public static FontStyleType AsStyleType(this FontStyle style)
        {
            switch (style)
            {
                case FontStyle.Italic:
                    return FontStyleType.Italic;
            }

            return FontStyleType.Normal;
        }
    }
}
