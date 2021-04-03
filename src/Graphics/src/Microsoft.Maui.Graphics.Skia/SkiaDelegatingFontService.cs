using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
    public class SkiaDelegatingFontService : AbstractFontService
    {
        public static SkiaDelegatingFontService Instance = new SkiaDelegatingFontService();

        private readonly Dictionary<string, SKTypeface> _typeFaces = new Dictionary<string, SKTypeface>();

        public override IFontFamily[] GetFontFamilies()
        {
            var nativeFontService = Fonts.GlobalService;
            return nativeFontService.GetFontFamilies();
        }

        public SKTypeface GetTypeface(string name)
        {
            var nativeFontService = Fonts.GlobalService;

            if (string.IsNullOrEmpty(name))
            {
                var style = nativeFontService.GetDefaultFontStyle();
                name = style.Name;
            }

            if (!_typeFaces.TryGetValue(name, out var typeface))
            {
                try
                {
                    var fontStyle = nativeFontService.GetFontStyleById(name);

                    var stream = fontStyle?.OpenStream();
                    if (stream != null)
                    {
                        typeface = SKTypeface.FromStream(new SKManagedStream(stream));
                    }
                    else if (fontStyle != null)
                    {
                        var slant = SKFontStyleSlant.Upright;
                        if (fontStyle.StyleType == FontStyleType.Italic)
                            slant = SKFontStyleSlant.Italic;
                        else if (fontStyle.StyleType == FontStyleType.Oblique)
                            slant = SKFontStyleSlant.Oblique;

                        typeface = SKTypeface.FromFamilyName(fontStyle.FontFamily.Name, fontStyle.Weight, (int) SKFontStyleWidth.Normal, slant);
                    }

                    if (typeface != null)
                        _typeFaces[name] = typeface;
                }
                catch (Exception exc)
                {
                    Logger.Info("Unable to load typeface [{0}]" + name, exc);
                }
            }

            return typeface;
        }

        public void ClearFontCache()
        {
            foreach (var entry in _typeFaces)
            {
                try
                {
                    entry.Value.Dispose();
                }
                catch (Exception exc)
                {
                    Logger.Info("Unable to dispose of a typeface", exc);
                }
            }

            _typeFaces.Clear();
        }
    }
}
