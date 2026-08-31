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
			string? temporaryFontPath = null;

			try
			{
				var fontPath = font.FontName;

				if (font.ResourceStream is null)
				{
					if (!File.Exists(fontPath))
						throw new InvalidOperationException("ResourceStream was null.");
				}
				else
				{
					temporaryFontPath = Path.ChangeExtension(
						Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()),
						Path.GetExtension(font.FontName));

					using (var temporaryFontStream = File.Create(temporaryFontPath))
						font.ResourceStream.CopyTo(temporaryFontStream);

					fontPath = temporaryFontPath;
				}

				using var provider = new CGDataProvider(fontPath);
				using var cgFont = CGFont.CreateFromProvider(provider);

				if (cgFont == null)
					throw new InvalidOperationException("Unable to load font from the stream.");

				var name = cgFont.PostScriptName;

				using var fontUrl = NSUrl.FromFilename(fontPath);
				var error = CTFontManager.RegisterFontsForUrl(fontUrl, CTFontManagerScope.Process);
				if (error is null)
					return name;

				var uiFont = UIFont.FromName(name, 10);
				if (uiFont != null)
					return name;

				throw new NSErrorException(error);
			}
			catch (Exception ex)
			{
				_serviceProvider?.CreateLogger<EmbeddedFontLoader>()?.LogWarning(ex, "Unable register font {Font} with the system.", font.FontName);
			}
			finally
			{
				if (temporaryFontPath is not null)
					DeleteTemporaryFont(temporaryFontPath);
			}

			return null;
		}

		void DeleteTemporaryFont(string fontPath)
		{
			try
			{
				File.Delete(fontPath);
			}
			catch (IOException ex)
			{
				_serviceProvider?.CreateLogger<EmbeddedFontLoader>()?.LogWarning(ex, "Unable to delete temporary font file {FontPath}.", fontPath);
			}
			catch (UnauthorizedAccessException ex)
			{
				_serviceProvider?.CreateLogger<EmbeddedFontLoader>()?.LogWarning(ex, "Unable to delete temporary font file {FontPath}.", fontPath);
			}
		}
	}
}
