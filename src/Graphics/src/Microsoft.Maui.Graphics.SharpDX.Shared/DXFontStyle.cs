using System;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX.DirectWrite;

namespace Microsoft.Maui.Graphics.SharpDX
{
    public class DXFontStyle : IFontStyle, IComparable
    {
        private readonly DXFontFamily _family;
        private readonly string _id;
        private readonly string _name;
        private readonly string _fullName;
        private readonly FontStyleType _styleType;
        private readonly FontStyle _dxFontStyle;
        private readonly FontWeight _dxFontWeight;
        private readonly int _weight;
        private readonly FontFace _dxFontFace;

        public DXFontStyle(DXFontFamily family, string id, string name, string fullName, FontStyle style, FontWeight weight, FontFace fontFace)
        {
            _family = family;
            _id = id;
            _name = name;
            _fullName = fullName;
            _styleType = style.AsStyleType();
            _weight = (int) weight;

            _dxFontStyle = style;
            _dxFontWeight = weight;
            _dxFontFace = fontFace;
        }

        public IFontFamily FontFamily => _family;

        public string Id => _id;

        public string Name => _name;

        public string FullName => _fullName;

        public FontStyleType StyleType => _styleType;

        public int Weight => _weight;

        public FontStyle DxFontStyle => _dxFontStyle;

        public FontWeight DxFontWeight => _dxFontWeight;

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(DXFontStyle))
                return false;
            DXFontStyle other = (DXFontStyle) obj;
            return _id == other._id;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                return _id?.GetHashCode() ?? 0;
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

            if (_name.Equals("Regular") || _name.Equals("Plain") || _name.Equals("Normal"))
            {
                return -1;
            }

            if (other.Name.Equals("Regular") || other.Name.Equals("Plain") || other.Name.Equals("Normal"))
            {
                return 1;
            }

            return string.Compare(_name, other.Name, StringComparison.Ordinal);
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as IFontStyle);
        }

        public Stream OpenStream()
        {
            var files = _dxFontFace?.GetFiles();
            if (files != null && files.Length > 0)
            {
                var file = files[0];
                var loader = file?.Loader;
                if (loader != null)
                {
                    using (var stream = loader.CreateStreamFromKey(file.GetReferenceKey()))
                    {
                        stream.ReadFileFragment(out var fragmentStart, 0, stream.GetFileSize(), out var fragmentContext);

                        var array = new byte[stream.GetFileSize()];
                        Marshal.Copy(fragmentStart, array, 0, (int) stream.GetFileSize());
                        var memoryStream = new MemoryStream(array);

                        stream.ReleaseFileFragment(fragmentContext);
                        return memoryStream;
                    }
                }
            }

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
