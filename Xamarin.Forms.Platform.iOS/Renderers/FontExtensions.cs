using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
#if __MOBILE__
using UIKit;
namespace Xamarin.Forms.Platform.iOS
#else
using AppKit;
using UIFont = AppKit.NSFont;

namespace Xamarin.Forms.Platform.MacOS
#endif
{
	public static class FontExtensions
	{
		static readonly Dictionary<ToUIFontKey, UIFont> ToUiFont = new Dictionary<ToUIFontKey, UIFont>();
#if __MOBILE__
		public static UIFont ToUIFont(this Font self)

#else
		public static UIFont ToNSFont(this Font self)
#endif
		{
			var size = (float)self.FontSize;
			if (self.UseNamedSize)
			{
				switch (self.NamedSize)
				{
					case NamedSize.Micro:
						size = 12;
						break;
					case NamedSize.Small:
						size = 14;
						break;
					case NamedSize.Medium:
						size = 17; // as defined by iOS documentation
						break;
					case NamedSize.Large:
						size = 22;
						break;
					default:
						size = 17;
						break;
				}
			}

			var bold = self.FontAttributes.HasFlag(FontAttributes.Bold);
			var italic = self.FontAttributes.HasFlag(FontAttributes.Italic);

			if (self.FontFamily != null)
			{
				try
				{
#if __MOBILE__
					if (UIFont.FamilyNames.Contains(self.FontFamily))
					{
						var descriptor = new UIFontDescriptor().CreateWithFamily(self.FontFamily);

						if (bold || italic)
						{
							var traits = (UIFontDescriptorSymbolicTraits)0;
							if (bold)
								traits = traits | UIFontDescriptorSymbolicTraits.Bold;
							if (italic)
								traits = traits | UIFontDescriptorSymbolicTraits.Italic;

							descriptor = descriptor.CreateWithTraits(traits);
							return UIFont.FromDescriptor(descriptor, size);
						}
					}

					return UIFont.FromName(self.FontFamily, size);

#else

					var descriptor = new NSFontDescriptor().FontDescriptorWithFamily(self.FontFamily);

					if (bold || italic)
					{
						var traits = (NSFontSymbolicTraits)0;
						if (bold)
							traits = traits | NSFontSymbolicTraits.BoldTrait;
						if (italic)
							traits = traits | NSFontSymbolicTraits.ItalicTrait;

						descriptor = descriptor.FontDescriptorWithSymbolicTraits(traits);
						return NSFont.FromDescription(descriptor, size);
					}

					return NSFont.FromFontName(self.FontFamily, size);
#endif
				}
				catch
				{
					Debug.WriteLine("Could not load font named: {0}", self.FontFamily);
				}
			}
			if (bold && italic)
			{
				var defaultFont = UIFont.SystemFontOfSize(size);
#if __MOBILE__
				var descriptor = defaultFont.FontDescriptor.CreateWithTraits(UIFontDescriptorSymbolicTraits.Bold | UIFontDescriptorSymbolicTraits.Italic);
				return UIFont.FromDescriptor(descriptor, 0);
			}
			if (italic)
				return UIFont.ItalicSystemFontOfSize(size);
#else
				var descriptor = defaultFont.FontDescriptor.FontDescriptorWithSymbolicTraits(
					NSFontSymbolicTraits.BoldTrait |
					NSFontSymbolicTraits.ItalicTrait);

				return NSFont.FromDescription(descriptor, 0);
			}
			if (italic)
			{
				Debug.WriteLine("Italic font requested, passing regular one");
				return NSFont.UserFontOfSize(size);
			}
#endif

			if (bold)
				return UIFont.BoldSystemFontOfSize(size);

			return UIFont.SystemFontOfSize(size);
		}

		internal static bool IsDefault(this Span self)
		{
			return self.FontFamily == null && self.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(Label), true) &&
					self.FontAttributes == FontAttributes.None;
		}

#if __MOBILE__
		internal static UIFont ToUIFont(this Label label)
#else
		internal static UIFont ToNSFont(this Label label)
#endif
		{
			var values = label.GetValues(Label.FontFamilyProperty, Label.FontSizeProperty, Label.FontAttributesProperty);
			return ToUIFont((string)values[0], (float)(double)values[1], (FontAttributes)values[2]) ??
					UIFont.SystemFontOfSize(UIFont.LabelFontSize);
		}

#if __MOBILE__
		internal static UIFont ToUIFont(this IFontElement element)
#else
		internal static NSFont ToNSFont(this IFontElement element)
#endif
		{
			return ToUIFont(element.FontFamily, (float)element.FontSize, element.FontAttributes);
		}

		static UIFont _ToUIFont(string family, float size, FontAttributes attributes)
		{
			var bold = (attributes & FontAttributes.Bold) != 0;
			var italic = (attributes & FontAttributes.Italic) != 0;

			if (family != null)
			{
				try
				{
					UIFont result;
#if __MOBILE__
					if (UIFont.FamilyNames.Contains(family))
					{
						var descriptor = new UIFontDescriptor().CreateWithFamily(family);

						if (bold || italic)
						{
							var traits = (UIFontDescriptorSymbolicTraits)0;
							if (bold)
								traits = traits | UIFontDescriptorSymbolicTraits.Bold;
							if (italic)
								traits = traits | UIFontDescriptorSymbolicTraits.Italic;

							descriptor = descriptor.CreateWithTraits(traits);
							result = UIFont.FromDescriptor(descriptor, size);
							if (result != null)
								return result;
						}
					}

					result = UIFont.FromName(family, size);
#else

					var descriptor = new NSFontDescriptor().FontDescriptorWithFamily(family);

					if (bold || italic)
					{
						var traits = (NSFontSymbolicTraits)0;
						if (bold)
							traits = traits | NSFontSymbolicTraits.BoldTrait;
						if (italic)
							traits = traits | NSFontSymbolicTraits.ItalicTrait;

						descriptor = descriptor.FontDescriptorWithSymbolicTraits(traits);
						result = NSFont.FromDescription(descriptor, size);
						if (result != null)
							return result;
					}

					result = NSFont.FromFontName(family, size);
#endif
					if (result != null)
						return result;
				}
				catch
				{
					Debug.WriteLine("Could not load font named: {0}", family);
				}
			}

			if (bold && italic)
			{
				var defaultFont = UIFont.SystemFontOfSize(size);

#if __MOBILE__
				var descriptor = defaultFont.FontDescriptor.CreateWithTraits(UIFontDescriptorSymbolicTraits.Bold | UIFontDescriptorSymbolicTraits.Italic);
				return UIFont.FromDescriptor(descriptor, 0);
			}
			if (italic)
				return UIFont.ItalicSystemFontOfSize(size);
#else
				var descriptor = defaultFont.FontDescriptor.FontDescriptorWithSymbolicTraits(
					NSFontSymbolicTraits.BoldTrait |
					NSFontSymbolicTraits.ItalicTrait);

				return NSFont.FromDescription(descriptor, 0);
			}
			if (italic)
			{
				var defaultFont = UIFont.SystemFontOfSize(size);
				var descriptor = defaultFont.FontDescriptor.FontDescriptorWithSymbolicTraits(NSFontSymbolicTraits.ItalicTrait);
				return NSFont.FromDescription(descriptor, 0);
			}
#endif
			if (bold)
				return UIFont.BoldSystemFontOfSize(size);

			return UIFont.SystemFontOfSize(size);
		}

		static UIFont ToUIFont(string family, float size, FontAttributes attributes)
		{
			var key = new ToUIFontKey(family, size, attributes);

			lock (ToUiFont)
			{
				UIFont value;
				if (ToUiFont.TryGetValue(key, out value))
					return value;
			}

			var generatedValue = _ToUIFont(family, size, attributes);

			lock (ToUiFont)
			{
				UIFont value;
				if (!ToUiFont.TryGetValue(key, out value))
					ToUiFont.Add(key, value = generatedValue);
				return value;
			}
		}

		struct ToUIFontKey
		{
			internal ToUIFontKey(string family, float size, FontAttributes attributes)
			{
				_family = family;
				_size = size;
				_attributes = attributes;
			}
#pragma warning disable 0414 // these are not called explicitly, but they are used to establish uniqueness. allow it!
			string _family;
			float _size;
			FontAttributes _attributes;
#pragma warning restore 0414
		}
	}
}