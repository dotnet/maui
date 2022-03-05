#nullable enable
using System;

namespace Microsoft.Maui
{
	public partial class EmbeddedFontLoader : IEmbeddedFontLoader
	{
		readonly IServiceProvider? _serviceProvider;

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
	}
}