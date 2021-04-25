#nullable enable
using System;
using CoreGraphics;
using CoreText;
using Foundation;
using Microsoft.Extensions.Logging;
using UIKit;

namespace Microsoft.Maui
{
	public class EmbeddedFontLoader : IEmbeddedFontLoader
	{
		readonly ILogger<EmbeddedFontLoader>? _logger;

		public EmbeddedFontLoader(ILogger<EmbeddedFontLoader>? logger = null)
		{
			_logger = logger;
		}

		public (bool success, string? filePath) LoadFont(EmbeddedFont font)
		{
			try
			{
				if (font.ResourceStream == null)
					throw new InvalidOperationException("ResourceStream was null.");

				var data = NSData.FromStream(font.ResourceStream);
				var provider = new CGDataProvider(data);
				var cGFont = CGFont.CreateFromProvider(provider);
				var name = cGFont.PostScriptName;

				if (CTFontManager.RegisterGraphicsFont(cGFont, out var error))
					return (true, name);

				var uiFont = UIFont.FromName(name, 10);
				if (uiFont != null)
					return (true, name);

				throw new NSErrorException(error);
			}
			catch (Exception ex)
			{
				_logger?.LogWarning(ex, "Unable register font {Font} with the system.", font.FontName);
			}

			return (false, null);
		}
	}
}