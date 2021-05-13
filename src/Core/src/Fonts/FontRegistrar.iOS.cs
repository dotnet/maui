#nullable enable
using System.IO;

namespace Microsoft.Maui
{
	public partial class FontRegistrar : IFontRegistrar
	{
		string? LoadNativeAppFont(string font, string filename, string? alias)
		{
			using var stream = GetNativeFontStream(filename, alias);

			return LoadEmbeddedFont(font, filename, alias, stream);
		}

		Stream GetNativeFontStream(string filename, string? alias)
		{
			var mainBundlePath = Foundation.NSBundle.MainBundle.BundlePath;

			var fontBundlePath = Path.Combine(mainBundlePath, filename);
			if (File.Exists(fontBundlePath))
				return File.OpenRead(fontBundlePath);

			fontBundlePath = Path.Combine(mainBundlePath, "Resources", filename);
			if (File.Exists(fontBundlePath))
				return File.OpenRead(fontBundlePath);

			fontBundlePath = Path.Combine(mainBundlePath, "Fonts", filename);
			if (File.Exists(fontBundlePath))
				return File.OpenRead(fontBundlePath);

			fontBundlePath = Path.Combine(mainBundlePath, "Resources", "Fonts", filename);
			if (File.Exists(fontBundlePath))
				return File.OpenRead(fontBundlePath);

			// TODO: check other folders as well

			throw new FileNotFoundException($"Native font with the name {filename} was not found.");
		}
	}
}