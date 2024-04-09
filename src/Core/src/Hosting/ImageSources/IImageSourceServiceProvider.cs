#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui
{
	public interface IImageSourceServiceProvider : IServiceProvider
	{
		IServiceProvider HostServiceProvider { get; }

		IImageSourceService? GetImageSourceService(Type imageSource);

#if !NETSTANDARD
		[Obsolete("Use GetImageSourceService instead.")]
		[RequiresDynamicCode("The GetImageSourceServiceType method is not AOT compatible. Use GetImageSourceService instead.")]
		[RequiresUnreferencedCode("The GetImageSourceServiceType method is not trimming compatible. Use GetImageSourceService instead.")]
		Type GetImageSourceServiceType(Type imageSource) => throw new NotImplementedException();

		[Obsolete("Use GetImageSourceService instead.")]
		[RequiresUnreferencedCode("The GetImageSourceType method is not trimming compatible. Use GetImageSourceService instead.")]
		Type GetImageSourceType(Type imageSource) => throw new NotImplementedException();
#else
		[Obsolete("Use GetImageSourceService instead.")]
		[RequiresUnreferencedCode("The GetImageSourceType method is not trimming compatible. Use GetImageSourceService instead.")]
		Type GetImageSourceServiceType(Type imageSource);

		[Obsolete("Use GetImageSourceService instead.")]
		[RequiresUnreferencedCode("The GetImageSourceType method is not trimming compatible. Use GetImageSourceService instead.")]
		Type GetImageSourceType(Type imageSource);
#endif
	}
}