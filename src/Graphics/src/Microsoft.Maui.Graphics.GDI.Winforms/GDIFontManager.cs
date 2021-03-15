using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;

namespace Microsoft.Maui.Graphics.GDI
{
    public static class GDIFontManager
    {
        private static readonly Dictionary<string, FontMapping> FontMapping = new Dictionary<string, FontMapping>();
        private static readonly FontMapping DefaultFont = new FontMapping("Arial", FontStyle.Regular);
        private static readonly List<FontInfo> FontInfoList = new List<FontInfo>();
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized) return;

            lock (FontMapping)
            {
                if (!_initialized)
                {
                    var fontCollection = new InstalledFontCollection();
                    foreach (var fontFamily in fontCollection.Families)
                    {
                        string name = fontFamily.Name;

                        var fontMapping = new FontMapping(name, FontStyle.Regular);
                        if (!FontMapping.ContainsKey(name))
                        {
                            FontMapping[name] = fontMapping;
                        }

                        var fontInfo = new FontInfo(name, name);
                        FontInfoList.Add(fontInfo);
                    }

                    _initialized = true;
                }
            }
        }

        public static FontMapping GetMapping(string postScriptName)
        {
            if (!_initialized) Initialize();

            if (postScriptName != null)
            {
                if (FontMapping.TryGetValue(postScriptName, out var mapping))
                {
                    return mapping;
                }
            }

            return DefaultFont;
        }

        public static IEnumerable<FontInfo> GetFonts()
        {
            if (!_initialized) Initialize();
            return FontInfoList;
        }
    }

    public class FontMapping
    {
        public readonly string Name;
        public readonly FontStyle Style;

        public FontMapping(string name, FontStyle style)
        {
            Name = name;
            Style = style;
        }
    }

    public class FontInfo
    {
        public readonly string Name;
        public readonly string PostscriptName;

        public FontInfo(string name, string postscriptName)
        {
            Name = name;
            PostscriptName = postscriptName;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}