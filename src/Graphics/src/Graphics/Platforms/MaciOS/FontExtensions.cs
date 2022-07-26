using CoreGraphics;
using CoreText;
using ObjCRuntime;
using System;
#if IOS || MACCATALYST || __IOS__
using PlatformFont = UIKit.UIFont;
#else
using PlatformFont = AppKit.NSFont;
#endif

namespace Microsoft.Maui.Graphics.Platform
{
	public static class FontExtensions
	{
		public static CGFont ToCGFont(this IFont font)
			=> string.IsNullOrEmpty(font?.Name)
				? GetDefaultCGFont()
				: CGFont.CreateWithFontName(font.Name);
			
		public static CTFont ToCTFont(this IFont font, nfloat? size = null)
			=> string.IsNullOrEmpty(font?.Name)
				? GetDefaultCTFont(size)
				: new CTFont(font.Name, size ?? PlatformFont.SystemFontSize, CTFontOptions.Default);

		public static PlatformFont ToPlatformFont(this IFont font, nfloat? size = null)
			=>
#if IOS || MACCATALYST || __IOS__
			PlatformFont.FromName(font?.Name ?? DefaultFontName, size ?? PlatformFont.SystemFontSize);
#else
			PlatformFont.FromFontName(font?.Name ?? DefaultFontName, size ?? PlatformFont.SystemFontSize);
#endif
		static string _defaultFontName;

		static string DefaultFontName
		{
			get
			{
				if (_defaultFontName == null)
				{
					using var defaultFont = PlatformFont.SystemFontOfSize(PlatformFont.SystemFontSize);
#if IOS || MACCATALYST || __IOS__
					_defaultFontName ??= defaultFont.Name;
#else
					_defaultFontName ??= defaultFont.FamilyName;
#endif
				}

				return _defaultFontName;
			}
		}


		internal static CGFont GetDefaultCGFont()
			=> CGFont.CreateWithFontName(DefaultFontName);

		internal static PlatformFont GetDefaultPlatformFont(nfloat? size = null)
			=> PlatformFont.SystemFontOfSize(size ?? PlatformFont.SystemFontSize);

		public static CTFont GetDefaultCTFont(nfloat? size = null)
			=> new CTFont(CTFontUIFontType.System, size ?? PlatformFont.SystemFontSize, Foundation.NSLocale.CurrentLocale.Identifier);


	}
}
