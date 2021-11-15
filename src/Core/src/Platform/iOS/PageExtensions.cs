using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui
{
	public static class PageExtensions
	{
		public static async Task UpdateBackgroundImageSourceAsync(this ContentView nativePage, IPage page, IImageSourceServiceProvider? provider)
		{
			if (provider == null)
				return;

			var backgroundImageSource = page.BackgroundImageSource;

			if (backgroundImageSource != null)
			{
				var service = provider.GetRequiredImageSourceService(backgroundImageSource);
				var result = await service.GetImageAsync(backgroundImageSource);
				var backgroundImage = result?.Value;

				if (backgroundImage == null)
					return;
	
				nativePage.BackgroundColor = UIColor.FromPatternImage(backgroundImage);
			}
		}
	}
}