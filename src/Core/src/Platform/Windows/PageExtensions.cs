using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui
{
	public static class PageExtensions
	{
		public static async Task UpdateBackgroundImageSourceAsync(this ContentPanel nativePage, IViewBackgroundImagePart viewBackgroundImagePart, IImageSourceServiceProvider? provider)
		{
			if (provider == null)
				return;

			var backgroundImageSource = viewBackgroundImagePart.Source;

			if (backgroundImageSource == null)
			{
				nativePage.Background = null;
				return;
			}

			if (provider != null && backgroundImageSource != null)
			{
				var service = provider.GetRequiredImageSourceService(backgroundImageSource);
				var nativeBackgroundImageSource = await service.GetImageSourceAsync(backgroundImageSource);

				nativePage.Background = new ImageBrush { ImageSource = nativeBackgroundImageSource?.Value };
			}
		}
	}
}
