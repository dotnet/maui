using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using CoreText;

namespace Microsoft.Maui.Graphics.Native
{
	public class NativeFontService : AbstractFontService
	{
		public static NativeFontService Instance = new NativeFontService();
		private static string _systemFontName;

		private readonly IFontFamily[] _fontFamilies;
		private readonly Dictionary<string, CTFont> _fontCache = new Dictionary<string, CTFont>();
		private readonly Dictionary<string, DateTime> _fontLastUsed = new Dictionary<string, DateTime>();

		public static string SystemFontName
		{
			get
			{
				if (_systemFontName == null)
				{
					var font = NSFont.SystemFontOfSize(NSFont.SystemFontSize);
					_systemFontName = font.FontName;
					font.Dispose();
				}

				return _systemFontName;
			}
		}

		protected NativeFontService()
		{
			_fontFamilies = InitializeFontFamilies();
		}

		public override IFontFamily[] GetFontFamilies()
		{
			return _fontFamilies;
		}

		public IFontFamily[] InitializeFontFamilies()
		{
			var familyNames = NSFontManager.SharedFontManager.AvailableFontFamilies;

			var families = new List<IFontFamily>();

			for (int i = 0; i < familyNames.Length; i++)
			{
				var familyName = familyNames[i];
				var family = new NativeFontFamily(familyName);
				if (family.GetFontStyles().Length > 0)
				{
					families.Add(family);
				}
			}

			families.Sort();
			return families.ToArray();
		}

		public CTFont LoadFont(string aName, float aSize)
		{
			CTFont vFont;

			if (aName == null) aName = "Helvetica";
			string vKey = aName + aSize;

			lock (_fontCache)
			{
				if (!_fontCache.TryGetValue(vKey, out vFont))
				{
					if (NativeFontRegistry.Instance.IsCustomFont(aName))
					{
						var vCGFont = NativeFontRegistry.Instance.GetCustomFont(aName);
						vFont = vCGFont != null
							? new CTFont(vCGFont, aSize, CGAffineTransform.MakeIdentity())
							: new CTFont("Helvetica", aSize);
					}
					else
					{
						vFont = new CTFont(aName, aSize);
					}

					_fontCache[vKey] = vFont;
				}
			}

			_fontLastUsed[vKey] = DateTime.UtcNow;
			return vFont;
		}

		public void ClearUnusedFonts(long maxAgeInSeconds)
		{
#if DEBUG
			Logger.Debug("Clearing font cache");
#endif

			lock (_fontCache)
			{
				var now = DateTime.UtcNow;

				List<string> keys = new List<string>(_fontLastUsed.Keys);
				foreach (var vKey in keys)
				{
					var lastUsed = _fontLastUsed[vKey];
					if (now.Subtract(lastUsed).Seconds > maxAgeInSeconds)
					{
						_fontLastUsed.Remove(vKey);
						//CTFont font = fontCache[vKey];
						//font.Dispose();
						_fontCache.Remove(vKey);

#if DEBUG
						Logger.Debug(" - Cleared font from cache: {0}", vKey);
#endif
					}
				}
			}
		}

		public void ClearFontCache()
		{
			lock (_fontCache)
			{
				/*foreach (var vFont in fontCache.Values)
				{
					vFont.Dispose();
				}*/

				_fontCache.Clear();
				_fontLastUsed.Clear();
			}
		}
	}
}
