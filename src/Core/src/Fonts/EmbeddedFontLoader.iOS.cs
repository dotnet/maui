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

#pragma warning disable CA1416  // TODO:  'RegisterGraphicsFont' is obsolete on: 'ios' 15.0 and later
#pragma warning disable CA1422
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
	}
}
