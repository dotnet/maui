using System;
using System.Diagnostics;
using CoreGraphics;
using CoreText;
using Foundation;
using UIKit;

namespace Microsoft.Maui
{
	public class EmbeddedFontLoader : IEmbeddedFontLoader
	{
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
				{
					return (true, name);
				}
				else //Lets check if the font is already registered
				{
					var uiFont = UIFont.FromName(name, 10);
					if (uiFont != null)
						return (true, name);
				}
				Debug.WriteLine(error.Description);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}

			return (false, null);
		}
	}
}