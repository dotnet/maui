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
			var assets = Android.App.Application.Context.Assets;

			if (assets != null && assets.Open(filename) is Stream assetStream)
				return assetStream;

			// TODO: check other folders as well

			throw new FileNotFoundException($"Native font with the name {filename} was not found.");
		}
	}
}