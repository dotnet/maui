using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;
using UIKit;

namespace Microsoft.Maui
{
	public class FontManager : IFontManager
	{
		// UIFontWeight[Constant] is internal in Xamarin.iOS but the convertion from
		// the public (int-based) enum is not helpful in this case.
		// -1.0 (Thin / 100) to 1.0 (Black / 900) with 0 being Regular (400)
		// which is not quite the center, not are the constant values linear
		static readonly (float value, FontWeight weight)[] FontWeightMap = new (float, FontWeight)[] {
			(-0.80f, FontWeight.Ultralight),
			(-0.60f, FontWeight.Thin),
			(-0.40f, FontWeight.Light),
			(0.0f, FontWeight.Regular),
			(0.23f, FontWeight.Medium),
			(0.30f, FontWeight.Semibold),
			(0.40f, FontWeight.Bold),
			(0.56f, FontWeight.Heavy),
			(0.62f, FontWeight.Black)
		};

		readonly ConcurrentDictionary<Font, UIFont> _fonts = new();
		readonly IFontRegistrar _fontRegistrar;
		readonly ILogger<FontManager>? _logger;

		UIFont? _defaultFont;

		public FontManager(IFontRegistrar fontRegistrar, ILogger<FontManager>? logger = null)
		{
			_fontRegistrar = fontRegistrar;
			_logger = logger;
		}

		public UIFont DefaultFont =>
			_defaultFont ??= UIFont.SystemFontOfSize(UIFont.SystemFontSize);

		public UIFont GetFont(Font font, double defaultFontSize = 0) =>
			GetFont(font, defaultFontSize, CreateFont);

		public double GetFontSize(Font font, double defaultFontSize = 0) =>
			font.FontSize <= 0
				? (defaultFontSize > 0 ? (float)defaultFontSize : DefaultFont.PointSize)
				: (nfloat)font.FontSize;

		static float GetWeightConstant(FontWeight self)
		{
			foreach (var (value, weight) in FontWeightMap)
			{
				if (self <= weight)
					return value;
			}
			return 1.0f;
		}

		UIFont GetFont(Font font, double defaultFont, Func<Font, UIFont> factory)
		{
			var size = GetFontSize(font, defaultFont);
			if (size != font.FontSize)
				font = font.WithSize(size);
			return _fonts.GetOrAdd(font, factory);
		}

		static UIFontAttributes GetFontAttributes(Font font)
		{
			var a = new UIFontAttributes
			{
				Traits = new UIFontTraits(),
			};
			var weight = font.Weight;
			if (font.Weight == 0)
				weight = FontWeight.Regular;
			var traits = (UIFontDescriptorSymbolicTraits)0;
			if (weight == FontWeight.Bold)
				traits |= UIFontDescriptorSymbolicTraits.Bold;
			else if (weight != FontWeight.Regular)
			{
				a.Traits = new UIFontTraits
				{
					Weight = GetWeightConstant(font.Weight),
					Slant = font.FontSlant == FontSlant.Oblique ? 30.0f : 0.0f
				};
			}
			if (font.FontSlant == FontSlant.Italic)
				traits |= UIFontDescriptorSymbolicTraits.Italic;

			a.Traits.SymbolicTrait = traits;
			return a;
		}

		UIFont CreateFont(Font font)
		{
			var family = font.FontFamily;
			var size = (nfloat)font.FontSize;

			var hasAttributes =
				font.Weight != FontWeight.Regular ||
				font.FontSlant != FontSlant.Default;

			if (family != null && family != DefaultFont.FamilyName)
			{
				try
				{
					UIFont? result = null;

					if (UIFont.FamilyNames.Contains(family))
					{
						var descriptor = new UIFontDescriptor().CreateWithFamily(family);
						if (hasAttributes)
							descriptor = descriptor.CreateWithAttributes(GetFontAttributes(font));

						result = UIFont.FromDescriptor(descriptor, size);
						if (result != null)
							return result;
					}

					var cleansedFont = CleanseFontName(family);
					result = UIFont.FromName(cleansedFont, size);
					if (result != null)
						return result;

					if (family.StartsWith(".SFUI", StringComparison.InvariantCultureIgnoreCase))
					{
						var fontWeight = family.Split('-').LastOrDefault();

						if (!string.IsNullOrWhiteSpace(fontWeight) && Enum.TryParse<UIFontWeight>(fontWeight, true, out var uIFontWeight))
						{
							result = UIFont.SystemFontOfSize(size, uIFontWeight);
							if (result != null)
								return result;
						}

						result = UIFont.SystemFontOfSize(size, UIFontWeight.Regular);
						if (result != null)
							return result;
					}

					result = UIFont.FromName(family, size);
					if (result != null)
						return result;
				}
				catch (Exception ex)
				{
					_logger?.LogWarning(ex, "Unable to load font '{Font}'.", family);
				}
			}

			if (hasAttributes)
			{
				var defaultFont = UIFont.SystemFontOfSize(size);
				var descriptor = defaultFont.FontDescriptor.CreateWithAttributes(GetFontAttributes(font));
				return UIFont.FromDescriptor(descriptor, size);
			}

			return UIFont.SystemFontOfSize(size);
		}

		string? CleanseFontName(string fontName)
		{
			// First check Alias
			if (_fontRegistrar.GetFont(fontName) is string fontPostScriptName)
				return fontPostScriptName;

			var fontFile = FontFile.FromString(fontName);

			if (!string.IsNullOrWhiteSpace(fontFile.Extension))
			{
				if (_fontRegistrar.GetFont(fontFile.FileNameWithExtension()) is string filePath)
					return filePath ?? fontFile.PostScriptName;
			}
			else
			{
				foreach (var ext in FontFile.Extensions)
				{
					var formatted = fontFile.FileNameWithExtension(ext);
					if (_fontRegistrar.GetFont(formatted) is string filePath)
						return filePath;
				}
			}

			return fontFile.PostScriptName;
		}
	}
}