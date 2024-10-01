#nullable enable
using System;
using System.IO;
using CoreGraphics;
using CoreText;
using Foundation;
using Microsoft.Extensions.Logging;
using UIKit;

namespace Microsoft.Maui
{

	/// <inheritdoc/>
	public partial class EmbeddedFontLoader
	{

		/// <inheritdoc/>
		public string? LoadFont(EmbeddedFont font)
		{
			try
			{
				var provider = GetCGDataProviderFromEmbeddedFont(font);
				CGFont? cgFont = CGFont.CreateFromProvider(provider);
				
				if (cgFont == null)
					throw new InvalidOperationException("Unable to load font from the stream.");

				var name = cgFont.PostScriptName;

				if (OperatingSystem.IsIOSVersionAtLeast(18, 0) || OperatingSystem.IsMacCatalystVersionAtLeast(18, 0))
				{
					var fontFilePath = SaveCGFontToFile(provider, name);
					if (string.IsNullOrEmpty(fontFilePath))
						return null;
					var fontUrl = NSUrl.FromFilename(fontFilePath);
					var nsError = CTFontManager.RegisterFontsForUrl(fontUrl, CTFontManagerScope.Process);
					if (nsError != null)
						throw new NSErrorException(nsError);
					return name;
				}

				if (CTFontManager.RegisterGraphicsFont(cgFont, out var error))
					return name;
#pragma warning restore CA1422 
#pragma warning restore CA1416

				var uiFont = UIFont.FromName(name, 10);
				if (uiFont != null)
					return name;

				if (error != null)
					throw new NSErrorException(error);
				else
					throw new InvalidOperationException("Unable to load font from the stream.");
			}
			catch (Exception ex)
			{
				_serviceProvider?.CreateLogger<EmbeddedFontLoader>()?.LogWarning(ex, "Unable register font {Font} with the system.", font.FontName);
			}

			return null;
		}

		private static CGDataProvider GetCGDataProviderFromEmbeddedFont(EmbeddedFont font)
		{
			if (font.ResourceStream == null)
			{
				if (!System.IO.File.Exists(font.FontName))
					throw new InvalidOperationException("ResourceStream was null.");

				return new CGDataProvider(font.FontName);
			}
			else
			{
				var data = NSData.FromStream(font.ResourceStream);
				if (data == null)
					throw new InvalidOperationException("Unable to load font stream data.");
				return new CGDataProvider(data);
			}
		}

		private static string? SaveCGFontToFile(CGDataProvider dataProvider, string? fontName)
		{
			if (string.IsNullOrEmpty(fontName))
				return null;

			using (var fontData = dataProvider.CopyData())
			{
				if (fontData == null)
					return null;

				var tempFolderPath = NSFileManager.DefaultManager.GetTemporaryDirectory().Path;
				if (string.IsNullOrEmpty(tempFolderPath))
					return null;

				var fontFilePath = System.IO.Path.Combine(tempFolderPath, $"{fontName}.ttf");

				System.IO.File.WriteAllBytes(fontFilePath, fontData.ToArray());
				return fontFilePath;
			}
		}
	}
}
