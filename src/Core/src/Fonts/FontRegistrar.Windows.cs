#nullable enable
using System.IO;

namespace Microsoft.Maui
{
	public partial class FontRegistrar : IFontRegistrar
	{
		string? LoadNativeAppFont(string font, string filename, string? alias)
		{
			var root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;

			var packagePath = Path.Combine(root, "Assets", filename);
			if (File.Exists(packagePath))
				return $"ms-appx:///Assets/{filename}";

			packagePath = Path.Combine(root, "Fonts", filename);
			if (File.Exists(packagePath))
				return $"ms-appx:///Fonts/{filename}";

			packagePath = Path.Combine(root, "Assets", "Fonts", filename);
			if (File.Exists(packagePath))
				return $"ms-appx:///Assets/Fonts/{filename}";

			// TODO: check other folders as well

			throw new FileNotFoundException($"Native font with the name {filename} was not found.");
		}
	}
}