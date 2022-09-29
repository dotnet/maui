#nullable enable
using System;
#if __ANDROID__
using System.IO;
#endif

namespace Microsoft.Maui
{
	/// <inheritdoc/>
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
		/// <summary>
		/// Creates a new <see cref="EmbeddedFontLoader"/> instance.
		/// </summary>
		public EmbeddedFontLoader()
			: this(null)
		{
		}

		/// <summary>
		/// Creates a new <see cref="EmbeddedFontLoader"/> instance.
		/// </summary>
		/// <param name="serviceProvider">The applications <see cref="IServiceProvider"/>.
		/// Typically this is provided through dependency injection.</param>
		public EmbeddedFontLoader(IServiceProvider? serviceProvider = null)
#if __ANDROID__
			: base(Path.GetTempPath, serviceProvider)
#endif
		{
			_serviceProvider = serviceProvider;
		}
	}
}