namespace Microsoft.Maui
{
	public class EmbeddedFontLoader : IEmbeddedFontLoader
	{
		public (bool success, string? filePath) LoadFont(EmbeddedFont font) => (false, null);
	}
}