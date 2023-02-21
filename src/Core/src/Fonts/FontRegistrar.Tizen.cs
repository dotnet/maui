#nullable enable
using System.IO;

namespace Microsoft.Maui
{
	/// <inheritdoc/>
	public partial class FontRegistrar : IFontRegistrar
	{
		string? LoadNativeAppFont(string font, string filename, string? alias)
		{
			using var stream = GetNativeFontStream(filename, alias);

			return LoadEmbeddedFont(font, filename, alias, stream);
		}

		Stream GetNativeFontStream(string filename, string? alias)
		{
			// TODO: check other folders as well
			var resDirPath = Tizen.Applications.Application.Current.DirectoryInfo.Resource;
			var fontPath = Path.Combine(resDirPath, "fonts", filename);
			if (File.Exists(fontPath))
				return File.OpenRead(fontPath);

			throw new FileNotFoundException($"Native font with the name {filename} was not found.");
		}
	}
}