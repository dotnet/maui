using System.IO;
using Windows.UI.Text;

namespace System.Graphics.Win2D
{
    public class W2DFontStyle : IFontStyle, IComparable
    {
        private readonly W2DFontFamily _family;

        public W2DFontStyle(W2DFontFamily family, string id, string name, string fullName, FontStyle style, FontWeight weight)
        {
            _family = family;
            Id = id;
            Name = name;
            FullName = fullName;
            StyleType = style.AsStyleType();
            Weight = weight.Weight;

            NativeFontStyle = style;
            NativeFontWeight = weight;
        }

        public IFontFamily FontFamily => _family;

        public string Id { get; }

        public string Name { get; }

        public string FullName { get; }

        public FontStyleType StyleType { get; }

        public int Weight { get; }

        public FontStyle NativeFontStyle { get; }

        public FontWeight NativeFontWeight { get; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(W2DFontStyle))
                return false;
            W2DFontStyle other = (W2DFontStyle)obj;
            return Id == other.Id;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                return (Id != null ? Id.GetHashCode() : 0);
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

            if (Name.Equals("Regular") || Name.Equals("Plain") || Name.Equals("Normal"))
            {
                return -1;
            }
            else if (other.Name.Equals("Regular") || other.Name.Equals("Plain") || other.Name.Equals("Normal"))
            {
                return 1;
            }

            return String.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as IFontStyle);
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
                case FontStyle.Oblique:
                    return FontStyleType.Oblique;
            }

            return FontStyleType.Normal;
        }
    }
}

