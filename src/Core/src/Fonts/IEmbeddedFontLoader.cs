#nullable enable
namespace Microsoft.Maui
{
	public interface IEmbeddedFontLoader
	{
		// TODO: this should be async as it involves copying files
		string? LoadFont(EmbeddedFont font);
	}
}