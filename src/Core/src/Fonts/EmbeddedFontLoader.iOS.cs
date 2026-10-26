#nullable enable
using System;
using System.IO;
using System.Security.Cryptography;
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
		const string FontCacheFolderName = "Microsoft.Maui.Fonts";
		static readonly object FontCacheLock = new();

		/// <inheritdoc/>
		public string? LoadFont(EmbeddedFont font)
		{
			string? cachedFontPath = null;
			var deleteCachedFontOnFailure = false;

			try
			{
				var fontPath = font.FontName;

				if (font.ResourceStream is null)
				{
					if (!File.Exists(fontPath))
						throw new FileNotFoundException($"Font file '{fontPath}' was not found.", fontPath);
				}
				else
				{
					(cachedFontPath, deleteCachedFontOnFailure) = CacheFont(font);
					fontPath = cachedFontPath;
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

				if (name is not null)
				{
					var uiFont = UIFont.FromName(name, 10);
					if (uiFont != null)
						return name;
				}

				throw new NSErrorException(error);
			}
			catch (Exception ex)
			{
				if (deleteCachedFontOnFailure && cachedFontPath is not null)
					DeleteTemporaryFont(cachedFontPath);

				_serviceProvider?.CreateLogger<EmbeddedFontLoader>()?.LogWarning(ex, "Unable register font {Font} with the system.", font.FontName);
			}

			return null;
		}

		(string FontPath, bool Created) CacheFont(EmbeddedFont font)
		{
			var cacheDirectory = Path.Combine(Path.GetTempPath(), FontCacheFolderName);
			Directory.CreateDirectory(cacheDirectory);

			var temporaryFontPath = Path.Combine(cacheDirectory, Path.GetRandomFileName());

			try
			{
				byte[] hash;

				using (var hashAlgorithm = SHA256.Create())
				{
					using (var temporaryFontStream = File.Create(temporaryFontPath))
					using (var hashingStream = new CryptoStream(temporaryFontStream, hashAlgorithm, CryptoStreamMode.Write))
					{
						font.ResourceStream!.CopyTo(hashingStream);
						hashingStream.FlushFinalBlock();
					}

					hash = hashAlgorithm.Hash!;
				}

				var extension = Path.GetExtension(font.FontName);
				var cachedFontName = Convert.ToHexString(hash);
				if (!string.IsNullOrEmpty(extension))
					cachedFontName += extension;

				var cachedFontPath = Path.Combine(cacheDirectory, cachedFontName);

				lock (FontCacheLock)
				{
					if (File.Exists(cachedFontPath))
					{
						DeleteTemporaryFont(temporaryFontPath);
						return (cachedFontPath, false);
					}

					File.Move(temporaryFontPath, cachedFontPath);
				}

				return (cachedFontPath, true);
			}
			catch
			{
				DeleteTemporaryFont(temporaryFontPath);
				throw;
			}
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
