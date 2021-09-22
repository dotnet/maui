using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Controls.Platform
{
	partial class ImageSourceLoader : IImageSourcePart
	{
		static Task<IImageSourceServiceResult<object>> GetNativeImage(IImageSource imageSource, IImageSourceService imageSourceService, IMauiContext mauiContext)
		{
			throw new NotImplementedException();
		}
	}
}
