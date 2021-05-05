#nullable enable

namespace Microsoft.Maui
{
	public partial class EmbeddedFontLoader
	{
		public (bool success, string? filePath) LoadFont(EmbeddedFont font) => (false, null);
	}
}