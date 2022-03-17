using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if ANDROID
using Android.Graphics.Drawables;
#endif

namespace Microsoft.Maui.DeviceTests
{
	public static class ImageSourceExtensions
	{
#if ANDROID
		public static async Task<Drawable> GetDrawableAsync(this IImageSourceService service, IImageSource imageSource)
		{
			var tcsDrawable = new TaskCompletionSource<Drawable>();

			// get an image
			var result = await service.LoadDrawableAsync(Platform.DefaultContext, imageSource, tcsDrawable.SetResult).ConfigureAwait(false);

			return await tcsDrawable.Task.ConfigureAwait(false);
		}

		public static async Task<(IImageSourceServiceResult<bool>,Drawable)> GetDrawableResultAsync(this IImageSourceService service, IImageSource imageSource)
		{
			var tcsDrawable = new TaskCompletionSource<Drawable>();

			// get an image
			var result = await service.LoadDrawableAsync(Platform.DefaultContext, imageSource, tcsDrawable.SetResult).ConfigureAwait(false);

			var drawable = await tcsDrawable.Task.ConfigureAwait(false);

			return (result, drawable);
		}
#endif
	}
}
