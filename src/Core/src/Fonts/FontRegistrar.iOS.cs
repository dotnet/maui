#nullable enable
using System.IO;

namespace Microsoft.Maui
{
	/// <inheritdoc/>
	public partial class FontRegistrar : IFontRegistrar
	{
		string? LoadNativeAppFont(string font, string filename, string? alias)
		{
			var resolvedFilename = ResolveFileSystemFont(filename);

			if (!string.IsNullOrEmpty(resolvedFilename))
				return LoadFileSystemFont(font, resolvedFilename, alias);

			return LoadEmbeddedFont(font, filename, alias, GetNativeFontStream(filename, alias));
		}

		string? ResolveFileSystemFont(string filename)
		{
			var mainBundlePath = Foundation.NSBundle.MainBundle.BundlePath;

#if MACCATALYST
			// macOS Apps have Contents folder in the bundle root, iOS does not
			mainBundlePath = Path.Combine(mainBundlePath, "Contents");
#endif

			var fontBundlePath = Path.Combine(mainBundlePath, filename);
			if (File.Exists(fontBundlePath))
				return fontBundlePath;

			fontBundlePath = Path.Combine(mainBundlePath, "Resources", filename);
			if (File.Exists(fontBundlePath))
				return fontBundlePath;

			fontBundlePath = Path.Combine(mainBundlePath, "Fonts", filename);
			if (File.Exists(fontBundlePath))
				return fontBundlePath;

			fontBundlePath = Path.Combine(mainBundlePath, "Resources", "Fonts", filename);
			if (File.Exists(fontBundlePath))
				return fontBundlePath;

			// TODO: check other folders as well

			return null;
		}

		Stream GetNativeFontStream(string filename, string? alias)
		{
			var resolvedFilename = ResolveFileSystemFont(filename);

			if (!string.IsNullOrEmpty(resolvedFilename) && File.Exists(resolvedFilename))
			{
				return File.OpenRead(resolvedFilename);
			}

			throw new FileNotFoundException($"Native font with the name {filename} was not found.");
		}
	}
}