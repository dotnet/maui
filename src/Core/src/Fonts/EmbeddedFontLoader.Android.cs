#nullable enable
using System.IO;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public class EmbeddedFontLoader : FileSystemEmbeddedFontLoader
	{
		public EmbeddedFontLoader(ILogger<EmbeddedFontLoader>? logger = null)
			: base(Path.GetTempPath(), logger)
		{
		}
	}
}