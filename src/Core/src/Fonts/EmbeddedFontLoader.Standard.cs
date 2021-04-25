#nullable enable
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public class EmbeddedFontLoader : IEmbeddedFontLoader
	{
		public EmbeddedFontLoader(ILogger<EmbeddedFontLoader>? logger = null)
		{
		}

		public (bool success, string? filePath) LoadFont(EmbeddedFont font) => (false, null);
	}
}