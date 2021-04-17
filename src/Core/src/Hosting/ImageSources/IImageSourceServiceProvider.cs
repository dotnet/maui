using System;

namespace Microsoft.Maui
{
	public interface IImageSourceServiceProvider : IServiceProvider
	{
		IImageSourceService? GetImageSourceService(Type imageSource);

		Type GetImageSourceServiceType(Type imageSource);

		Type GetImageSourceType(Type imageSource);
	}
}