using System;
using System.Collections.Generic;
using System.IO;
using Android.App;
using Android.Graphics;

namespace Microsoft.Maui.Graphics.Android
{
    public class NativeFontService : AbstractFontService
    {
        public static Boolean FontAliasingEnabled { get; set; } = true;
        
        public const string SystemFont = "System";
        public const string SystemBoldFont = "System-Bold";

        public static NativeFontService Instance = new NativeFontService();

        private IFontFamily[] _fontFamilies;
        private readonly Dictionary<string, Typeface> _typeFaces = new Dictionary<string, Typeface>();

        public NativeFontService() : base("Droid Sans", "Roboto", "Aramo")
        {
        }

        private static readonly List<string> ExcludeList = new List<string>
        {
            "AndroidClock",
            "AndroidClock-Large",
            "Clockopia",
            "Droid Sans Fallback",
            "Lohit Bengali",
            "Lohit Devanagari",
            "Lohit Tamil",
            "Roboto Test1",
            "GS_Thai",
            "GS45_Arab(AndroidOS)",
            "Symbol Std"
        };

        public override IFontFamily[] GetFontFamilies()
        {
            return _fontFamilies ?? (_fontFamilies = InitializeFonts());
        }

        public IFontFamily[] InitializeFonts()
        {
            var families = new Dictionary<string, NativeFontFamily>();
            var familyList = new List<IFontFamily>();
            var analyzer = new FontAnalyzer();

            var assembly = typeof(NativeFontService).Assembly;
            var resources = assembly.GetManifestResourceNames();
            foreach (var resource in resources)
            {
                if (resource.EndsWith("tf", StringComparison.OrdinalIgnoreCase))
                {
                    var path = resource.Split('.');
                    var id = path[path.Length - 2];
                    var parts = id.Split('-');
                    var familyName = parts[0];
                    var type = parts[1];

                    if (familyName.StartsWith("TeXGyre", StringComparison.InvariantCulture))
                        familyName = "TeX Gyre " + familyName.Substring(7);

                    if (!families.TryGetValue(familyName, out var family))
                    {
                        family = new NativeFontFamily(familyName);
                        families[familyName] = family;
                        familyList.Add(family);
                    }

                    var weight = FontUtils.Regular;
                    if (type.Contains("Bold"))
                        weight = FontUtils.Bold;

                    var styleType = FontStyleType.Normal;
                    if (type.Contains("Italic"))
                        styleType = FontStyleType.Italic;

                    var fullName = $"{familyName} {type}";
                    if ("Regular".Equals(type))
                        fullName = familyName;

                    var style = new NativeFontStyle(family, id, type, fullName, weight, styleType, resource, true);
                    family.AddStyle(style);

                    if (FontAliasingEnabled)
                    {
                        var suffix = string.Empty;
                        var italic = "Italic";
                        if ("Arimo".Equals(familyName))
                        {
                            familyName = "Arial";
                            id = "Arial";
                            suffix = "MT";
                        }
                        else if ("Tinos".Equals(familyName))
                        {
                            familyName = "Times New Roman";
                            id = "TimesNewRomanPS";
                            suffix = "MT";
                        }
                        else if ("Cousine".Equals(familyName))
                        {
                            familyName = "Courier New";
                            id = "CourierNewPS";
                            suffix = "MT";
                        }
                        else if ("TeX Gyre Termes".Equals(familyName))
                        {
                            familyName = "Times";
                            id = "Times";
                            suffix = "";
                        }
                        else if ("TeX Gyre Heros".Equals(familyName))
                        {
                            familyName = "Helvetica";
                            id = "Helvetica";
                            italic = "Oblique";
                            suffix = "";
                        }
                        else if ("TeX Gyre Cursor".Equals(familyName))
                        {
                            familyName = "Courier";
                            id = "Courier";
                            italic = "Oblique";
                            suffix = "";
                        }

                        if (!families.TryGetValue(familyName, out family))
                        {
                            family = new NativeFontFamily(familyName);
                            families[familyName] = family;
                            familyList.Add(family);
                        }

                        fullName = $"{familyName} {type}";
                        if ("Regular".Equals(type))
                            fullName = familyName;

                        if (styleType == FontStyleType.Italic)
                        {
                            if (weight == FontUtils.Bold)
                                id = id + "-" + "Bold" + italic + suffix;
                            else
                                id = id + "-" + italic + suffix;

                            if ("Oblique".Equals(italic))
                                styleType = FontStyleType.Oblique;
                        }
                        else if (weight == FontUtils.Bold)
                        {
                            id = id + "-" + "Bold" + suffix;
                        }

                        style = new NativeFontStyle(family, id, type, fullName, weight, styleType, resource, true);
                        family.AddStyle(style);
                    }
                }
            }

            foreach (string searchPath in FontSearchPaths)
            {
                if (searchPath != null)
                {
                    var searchDirectory = new DirectoryInfo(searchPath);

                    if (searchDirectory.Exists)
                    {
                        var files = searchDirectory.GetFiles();
                        foreach (var file in files)
                        {
                            try
                            {
                                var fontInfo = analyzer.GetFontInfo(file.FullName);

                                if (fontInfo != null)
                                {
                                    if (IsValidFont(fontInfo))
                                    {
                                        if (!families.TryGetValue(fontInfo.Family, out var family))
                                        {
                                            family = new NativeFontFamily(fontInfo.Family);
                                            families[fontInfo.Family] = family;
                                            familyList.Add(family);
                                        }

                                        if (!family.HasStyle(fontInfo.Style))
                                        {
                                            var id = fontInfo.FullName;
                                            var name = fontInfo.Style;
                                            var weight = FontUtils.GetFontWeight(name);
                                            var styleType = FontUtils.GetStyleType(name);

                                            string fullName = fontInfo.Family;
                                            if (!"Regular".Equals(fontInfo.Style))
                                                fullName = $"{fontInfo.Family} {name}";

                                            var style = new NativeFontStyle(family, id, name, fullName, weight, styleType, fontInfo.Path);
                                            family.AddStyle(style);
                                        }
                                        else
                                        {
                                            Logger.Info("Duplicate style found for font: {0} {1}", fontInfo.Family, fontInfo.Style);
                                        }
                                    }
                                }
                                else
                                {
                                    Logger.Info("Unable to load the font info for the font file: " + file.FullName);
                                }
                            }
                            catch (Exception exc)
                            {
                                Logger.Info("Unable to handle the font file: " + file.FullName, exc);
                            }
                        }
                    }
                }
            }

            familyList.Sort();
            return familyList.ToArray();
        }

        protected bool IsValidFont(FontInfo fontInfo)
        {
            if (fontInfo.Family == null)
                return false;

            if (ExcludeList.Contains(fontInfo.Family))
                return false;

            if (fontInfo.Family.StartsWith("Samsung"))
                return false;

            return true;
        }

        /// <summary>
        /// Returns the list of paths that the font service should search for font files.
        /// </summary>
        /// <value>The font search paths.</value>
        protected string[] FontSearchPaths => new[] {"/system/fonts", "/system/font", "/data/fonts", UserFontsPath};

        /// <summary>
        /// The path where application user added fonts are (or should be) installed.
        /// </summary>
        /// <value>The application fonts path.</value>
        protected string UserFontsPath
        {
            get
            {
                string fontsPath = null;

                try
                {
                    var externalFilesFir = Application.Context.GetExternalFilesDir(null);
                    var absolutePath = externalFilesFir.AbsolutePath;
                    fontsPath = System.IO.Path.Combine(absolutePath, "Fonts");
                }
                catch (Exception exc)
                {
                    Logger.Debug(exc);
                }

                return fontsPath;
            }
        }

        public Typeface GetTypeface(string name)
        {
            if (name == null || SystemFont.Equals(name))
                return Typeface.Default;

            if (SystemBoldFont.Equals(name))
                return Typeface.DefaultBold;

            if (GetFontStyleById(name) is NativeFontStyle fontStyle)
            {
                if (!_typeFaces.TryGetValue(name, out var typeface))
                {
                    string path;

                    if (fontStyle.Resource)
                    {
                        var resource = fontStyle.Path;
                        var resourceParts = resource.Split('.');
                        var fileName = $"{resourceParts[resourceParts.Length - 2]}.{resourceParts[resourceParts.Length - 1]}";

                        path = System.IO.Path.Combine(UserFontsPath, fileName);
                        if (!File.Exists(path))
                        {
                            if (!Directory.Exists(UserFontsPath))
                                Directory.CreateDirectory(UserFontsPath);

                            var assembly = typeof(NativeFontService).Assembly;
                            using (var stream = assembly.GetManifestResourceStream(resource))
                            {
                                using (var outputStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                                {
                                    stream?.CopyTo(outputStream);
                                }
                            }
                        }
                    }
                    else
                    {
                        path = fontStyle.Path;
                    }

                    try
                    {
                        typeface = Typeface.CreateFromFile(path);
                        if (typeface != null)
                            _typeFaces[name] = typeface;
                    }
                    catch (Java.Lang.RuntimeException exc)
                    {
                        Logger.Info("Unable to load font from the file: " + fontStyle.Path, exc);
                    }
                    catch (Exception exc)
                    {
                        Logger.Info("Unable to load font from the file: " + fontStyle.Path, exc);
                    }
                }

                if (typeface != null)
                    return typeface;
            }

            return Typeface.Default;
        }

        public string GetFontPath(string name)
        {
            if (name != null)
            {
                if (GetFontStyleById(name) is NativeFontStyle fontStyle)
                    return fontStyle.Path;
            }

            return null;
        }

        public void ClearFontCache()
        {
            _fontFamilies = null;

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