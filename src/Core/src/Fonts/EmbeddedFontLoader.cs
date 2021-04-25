#nullable enable
using System.IO;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class EmbeddedFontLoader : IEmbeddedFontLoader
	{
		readonly ILogger<EmbeddedFontLoader>? _logger;

		public EmbeddedFontLoader()
			: this(null)
		{
		}

		public EmbeddedFontLoader(ILogger<EmbeddedFontLoader>? logger = null)
#if __ANDROID__
			: base(Path.GetTempPath(), logger)
#endif
		{
			_logger = logger;
		}
	}
}