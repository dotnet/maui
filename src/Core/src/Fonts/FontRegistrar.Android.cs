#nullable enable
using System.IO;

namespace Microsoft.Maui
{
	/// <inheritdoc/>
	public partial class FontRegistrar : IFontRegistrar
	{
		// Return the filename as-is, as we load the font directly in FontManager
		string? LoadNativeAppFont(string font, string filename, string? alias) => filename;
	}
}