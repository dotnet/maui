#nullable enable
using System;
using System.IO;

namespace Microsoft.Maui
{
	public partial class EmbeddedFontLoader : IEmbeddedFontLoader
	{
		readonly IServiceProvider? _serviceProvider;

#if !NET6_0_OR_GREATER
		// The .NET 6+ linker won't need this
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

		public EmbeddedFontLoader(IServiceProvider? serviceProvider = null)
#if __ANDROID__
			: base(Path.GetTempPath, serviceProvider)
#endif
		{
			_serviceProvider = serviceProvider;
		}
	}
}