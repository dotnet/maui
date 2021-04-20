using System;
using System.Collections.Generic;
using CoreGraphics;
using CoreText;
using UIKit;

namespace Microsoft.Maui.Graphics.Native
{
	public class NativeFontService : AbstractFontService
	{
		public static NativeFontService Instance = new NativeFontService();

		private CGFont _systemFont;
		private CGFont _boldSystemFont;

		private readonly Dictionary<string, CTFont> _fontCache = new Dictionary<string, CTFont>();
		private readonly IFontFamily[] _fontFamilies;
		private readonly string _systemFontName;
		private readonly string _boldSystemFontName;

		protected NativeFontService()
		{
			_fontFamilies = InitializeFontFamilies();

			var font = UIFont.SystemFontOfSize(12);
			_systemFontName = font.Name;
			font.Dispose();

			var boldFont = UIFont.BoldSystemFontOfSize(12);
			_boldSystemFontName = boldFont.Name;
			boldFont.Dispose();
		}

		public override IFontFamily[] GetFontFamilies()
		{
			return _fontFamilies;
		}

		public IFontFamily[] InitializeFontFamilies()
		{
			var familyNames = UIFont.FamilyNames;

			var families = new List<IFontFamily>();

			foreach (var familyName in familyNames)
			{
				var family = new NativeFontFamily(familyName);
				if (family.GetFontStyles().Length > 0)
					families.Add(family);
			}

			families.Sort();
			return families.ToArray();
		}

		public CTFont LoadFont(string name, float size)
		{
			if (name == null)
				return LoadFont("Arial", size);

			string key = name + size;
			CTFont font = GetFontFromCache(key, name, size);

			// Make sure it hasn't been disposed.
			if (font.Handle == IntPtr.Zero)
			{
				lock (_fontCache)
					_fontCache.Remove(key);
				font = GetFontFromCache(key, name, size);
			}

			return font;
		}

		private CTFont GetFontFromCache(string key, string name, float size)
		{
			CTFont font;

			lock (_fontCache)
			{
				if (!_fontCache.TryGetValue(key, out font))
				{
					/*if (name != null && MTFontRegistry.Instance.IsCustomFont(name))
					{
						var coreGraphicsFont = MTFontRegistry.Instance.GetCustomFont(name);
						if (coreGraphicsFont != null)
						{
							font = new CTFont(coreGraphicsFont, size, CGAffineTransform.MakeIdentity());
						}
					}
					else
					{*/
					var fontStyle = GetFontStyleById(name);
					if (fontStyle == null)
					{
						if (_systemFontName.Equals(name))
						{
							if (name.StartsWith(".", StringComparison.InvariantCultureIgnoreCase))
							{
								var cgfont = _systemFont ?? (_systemFont = CGFont.CreateWithFontName(name));
								font = new CTFont(cgfont, size, CGAffineTransform.MakeIdentity());
							}
							else
							{
								font = new CTFont(name, size);
							}
						}
						else if (_boldSystemFontName.Equals(name))
						{
							if (name.StartsWith(".", StringComparison.InvariantCultureIgnoreCase))
							{
								var cgfont = _boldSystemFont ?? (_boldSystemFont = CGFont.CreateWithFontName(name));
								font = new CTFont(cgfont, size, CGAffineTransform.MakeIdentity());
							}
							else
							{
								font = new CTFont(name, size);
							}
						}
						else
						{
							fontStyle = GetDefaultFontStyle();
						}
					}

					if (font == null)
						font = new CTFont(fontStyle.Id, size);
					//}

					if (font == null)
						return LoadFont("ArialMT", size);

					_fontCache[key] = font;
				}
			}

			return font;
		}

		public void ClearFontCache()
		{
#if DEBUG
			Logger.Debug("Clearing font cache.");
#endif

			lock (_fontCache)
			{
				foreach (var font in _fontCache.Values)
				{
					font.Dispose();
				}

				_fontCache.Clear();
			}
		}
	}
}
