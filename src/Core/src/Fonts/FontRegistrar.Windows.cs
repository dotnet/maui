#nullable enable
using System.IO;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui
{
	/// <inheritdoc/>
	public partial class FontRegistrar : IFontRegistrar
	{
		string? LoadNativeAppFont(string font, string filename, string? alias)
		{
			if (FileSystemUtils.AppPackageFileExists(filename))
				return $"ms-appx:///{filename}";

			var packagePath = Path.Combine("Assets", filename);
			if (FileSystemUtils.AppPackageFileExists(packagePath))
				return $"ms-appx:///Assets/{filename}";

			packagePath = Path.Combine("Fonts", filename);
			if (FileSystemUtils.AppPackageFileExists(packagePath))
				return $"ms-appx:///Fonts/{filename}";

			packagePath = Path.Combine("Assets", "Fonts", filename);
			if (FileSystemUtils.AppPackageFileExists(packagePath))
				return $"ms-appx:///Assets/Fonts/{filename}";

			// TODO: check other folders as well

			throw new FileNotFoundException($"Native font with the name {filename} was not found.");
		}
	}
}