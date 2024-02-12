#nullable enable

using System;

namespace Microsoft.Maui
{
	public interface IImageSourceServiceProvider : IServiceProvider
	{
		IServiceProvider HostServiceProvider { get; }

		IImageSourceService? GetImageSourceService(Type imageSource);

#if !NETSTANDARD
		[Obsolete("Use GetImageSourceService instead.")]
		Type GetImageSourceServiceType(Type imageSource) => throw new NotImplementedException();

		[Obsolete("Use GetImageSourceService instead.")]
		Type GetImageSourceType(Type imageSource) => throw new NotImplementedException();
#else
		[Obsolete("Use GetImageSourceService instead.")]
		Type GetImageSourceServiceType(Type imageSource);

		[Obsolete("Use GetImageSourceService instead.")]
		Type GetImageSourceType(Type imageSource);
#endif
	}
}