#nullable enable
using System;
using System.IO;

namespace Microsoft.Maui
{
	/// <inheritdoc/>
	public partial class FontRegistrar : IFontRegistrar
	{
		// There is no platform font system on Standard, but an app host (for example the Linux/GTK 4 backend) can supply an
		// IEmbeddedFontLoader. Fonts registered by file name (ConfigureFonts: AddFont("Name.ttf")) are the files MauiFont
		// copies next to the application, in Resources/Fonts. They are handed to the loader as a stream; the default Standard
		// loader returns null, so without a host the font stays unresolved as before.
		string? LoadNativeAppFont(string font, string filename, string? alias)
		{
			var path = FindNativeFontFile(filename);
			if (path is null)
				return null;

			using var stream = File.OpenRead(path);

			return LoadEmbeddedFont(font, filename, alias, stream);
		}

		static string? FindNativeFontFile(string filename)
		{
			var name = Path.GetFileName(filename);
			if (string.IsNullOrEmpty(name))
				return null;

			var baseDirectory = AppContext.BaseDirectory;
			string[] folders =
			{
				Path.Combine(baseDirectory, "Resources", "Fonts"),
				Path.Combine(baseDirectory, "Fonts"),
				baseDirectory,
			};

			foreach (var folder in folders)
			{
				var candidate = Path.Combine(folder, name);
				if (File.Exists(candidate))
					return candidate;
			}

			return null;
		}
	}
}
