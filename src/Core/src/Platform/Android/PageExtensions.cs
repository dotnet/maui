using System.Threading.Tasks;
using Android.Graphics.Drawables;

namespace Microsoft.Maui
{
	public static class PageExtensions
	{
		public static async Task UpdateBackgroundImageSourceAsync(this ContentViewGroup nativePage, IViewBackgroundImagePart viewBackgroundImagePart, IImageSourceServiceProvider? provider)
		{
			if (provider == null)
				return;

			var context = nativePage.Context;

			if (context == null)
				return;

			var backgroundImageSource = viewBackgroundImagePart.Source;

			if (backgroundImageSource != null)
			{
				var service = provider.GetRequiredImageSourceService(backgroundImageSource);
				var result = await service.GetDrawableAsync(backgroundImageSource, context);
				Drawable? backgroundImageDrawable = result?.Value;

				if (nativePage.IsAlive())
					nativePage.Background = backgroundImageDrawable;
			}
		}
	}
}