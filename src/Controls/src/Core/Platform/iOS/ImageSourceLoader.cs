
using System;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	partial class ImageSourceLoader : IImageSourcePart
	{
		static Task<IImageSourceServiceResult<UIImage>> GetNativeImage(IImageSource imageSource, IImageSourceService imageSourceService, IMauiContext mauiContext)
		{
			return imageSourceService.GetImageAsync(imageSource);
		}
	}
}