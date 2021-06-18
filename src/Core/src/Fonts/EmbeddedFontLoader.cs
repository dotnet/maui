#nullable enable
using System.IO;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class EmbeddedFontLoader : IEmbeddedFontLoader
	{
		readonly ILogger<EmbeddedFontLoader>? _logger;

#if !NET6_0
		// The NET6_0 linker won't need this
		// Make sure to test with full linking on before removing
#if __ANDROID__
		[Android.Runtime.Preserve]
#elif __IOS__
		[Foundation.Preserve]
#endif
#endif
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