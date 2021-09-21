using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Controls.Platform
{
	partial class ImageSourceLoader
	{
		static Task<IImageSourceServiceResult<object>> GetNativeImageAsync(IImageSource imageSource, IImageSourceService imageSourceService, IMauiContext mauiContext)
		{
			throw new NotImplementedException();
		}
	}
}
