using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
    public class SkiaFontService : AbstractFontService
    {
        private readonly Dictionary<string, SKTypeface> _typeFaces = new Dictionary<string, SKTypeface>();
        private readonly IFontFamily[] _fontFamilies;
        private readonly string _systemFontName;
        private readonly string _boldSystemFontName;

        public SkiaFontService(string systemFontName, string boldSystemFontName)
        {
            _systemFontName = systemFontName;
            _boldSystemFontName = boldSystemFontName;

            _fontFamilies = InitializeFontFamilies();
        }

        public override IFontFamily[] GetFontFamilies()
        {
            return _fontFamilies;
        }

        public SKTypeface GetTypeface(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = _systemFontName;

            if (!_typeFaces.TryGetValue(name, out var typeface))
            {
                try
                {
                    typeface = SKTypeface.FromFamilyName(name);

                    if (typeface != null)
                        _typeFaces[name] = typeface;
                }
                catch (Exception exc)
                {
                    typeface = SKTypeface.FromFamilyName(_systemFontName);
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

        public IFontFamily[] InitializeFontFamilies()
        {
            var familyNames = SKFontManager.Default.GetFontFamilies();

            var families = new List<IFontFamily>();

            foreach (var familyName in familyNames)
            {
                var family = new SkiaFontFamily(familyName);
                if (family.GetFontStyles().Length > 0)
                    families.Add(family);
            }

            families.Sort();
            return families.ToArray();
        }
    }
}