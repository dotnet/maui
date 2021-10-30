#nullable enable

using System;

namespace Microsoft.Maui
{
	public interface IImageSourceServiceProvider : IServiceProvider
	{
		IServiceProvider HostServiceProvider { get; }

		IImageSourceService? GetImageSourceService(Type imageSource);

		Type GetImageSourceServiceType(Type imageSource);

		Type GetImageSourceType(Type imageSource);
	}
}