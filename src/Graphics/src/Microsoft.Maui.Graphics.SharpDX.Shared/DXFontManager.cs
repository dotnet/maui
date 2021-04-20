using System.Collections.Generic;
using SharpDX.DirectWrite;

namespace Microsoft.Maui.Graphics.SharpDX
{
	public static class DXFontManager
	{
		private static readonly Dictionary<string, FontMapping> FontMapping = new Dictionary<string, FontMapping>();
		private static readonly FontMapping DefaultFont = new FontMapping("Arial", FontStyle.Normal, FontWeight.Normal);
		private static readonly List<FontInfo> FontInfoList = new List<FontInfo>();
		private static bool _initialized;

		public static void Initialize()
		{
			if (_initialized) return;

			lock (FontMapping)
			{
				if (!_initialized)
				{
					var fontCollection = DXGraphicsService.FactoryDirectWrite.GetSystemFontCollection(false);
					int familyCount = fontCollection.FontFamilyCount;

					for (int i = 0; i < familyCount; i++)
					{
						var fontFamily = fontCollection.GetFontFamily(i);
						var familyNames = fontFamily.FamilyNames;

						string name = familyNames.GetString(0);

						for (int j = 0; j < fontFamily.FontCount; j++)
						{
							string postScriptName = name;

							var font = fontFamily.GetFont(j);

							var found = font.GetInformationalStrings(InformationalStringId.PostscriptName, out var localizedPostScriptName);
							if (found)
							{
								postScriptName = localizedPostScriptName.GetString(0);
							}

							var fontMapping = new FontMapping(name, font.Style, font.Weight);
							if (!FontMapping.ContainsKey(postScriptName))
							{
								FontMapping[postScriptName] = fontMapping;
							}

							var fontInfo = new FontInfo(name, postScriptName);
							FontInfoList.Add(fontInfo);
						}
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
		public FontWeight Weight;

		public FontMapping(string name, FontStyle style, FontWeight weight)
		{
			Name = name;
			Style = style;
			Weight = weight;
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
