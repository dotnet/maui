using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui
{
	public static class PageExtensions
	{
		public static async Task UpdateBackgroundImageSourceAsync(this ContentPanel nativePage, IPage page, IImageSourceServiceProvider? provider)
		{
			if (provider == null)
				return;

			var backgroundImageSource = page.BackgroundImageSource;

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
