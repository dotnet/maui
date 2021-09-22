
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Platform
{
	partial class ImageSourceLoader : IImageSourcePart
	{
		static Task<IImageSourceServiceResult<UI.Xaml.Media.ImageSource>> GetNativeImage(IImageSource imageSource, IImageSourceService imageSourceService, IMauiContext mauiContext)
		{
			return imageSourceService.GetImageSourceAsync(imageSource);
		}
	}
}