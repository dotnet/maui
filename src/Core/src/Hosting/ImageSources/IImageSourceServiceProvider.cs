using System;

namespace Microsoft.Maui
{
	public interface IImageSourceServiceProvider : IServiceProvider
	{
		IImageSourceService? GetImageSourceService(IImageSource imageSource);

		Type GetImageSourceType(IImageSource imageSource);
	}
}