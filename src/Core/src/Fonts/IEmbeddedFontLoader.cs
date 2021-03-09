namespace Microsoft.Maui
{
	public interface IEmbeddedFontLoader
	{
		(bool success, string? filePath) LoadFont(EmbeddedFont font);
	}
}