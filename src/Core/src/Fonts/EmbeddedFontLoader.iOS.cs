#nullable enable
using System;
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
				CGFont? cgFont;

				if (font.ResourceStream == null)
				{
					if (!System.IO.File.Exists(font.FontName))
						throw new InvalidOperationException("ResourceStream was null.");

					var provider = new CGDataProvider(font.FontName);
					cgFont = CGFont.CreateFromProvider(provider);
				}
				else
				{
					var data = NSData.FromStream(font.ResourceStream);
					if (data == null)
						throw new InvalidOperationException("Unable to load font stream data.");
					var provider = new CGDataProvider(data);
					cgFont = CGFont.CreateFromProvider(provider);
				}

				if (cgFont == null)
					throw new InvalidOperationException("Unable to load font from the stream.");

				var name = cgFont.PostScriptName;

#pragma warning disable CA1422 //obsolete on iOS 18.0
				if (CTFontManager.RegisterGraphicsFont(cgFont, out var error))
					return name;
#pragma warning restore CA1422

				var uiFont = UIFont.FromName(name, 10);
				if (uiFont != null)
					return name;

				// we know error is not null, the NotNullWhen attr is missing in the iOS bindings, ref: https://github.com/xamarin/xamarin-macios/pull/20050
				throw new NSErrorException(error!);
			}
			catch (Exception ex)
			{
				_serviceProvider?.CreateLogger<EmbeddedFontLoader>()?.LogWarning(ex, "Unable register font {Font} with the system.", font.FontName);
			}

			return null;
		}
	}
}
