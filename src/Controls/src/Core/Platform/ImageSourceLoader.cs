using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIImage;
#elif MONOANDROID
using NativeView = Android.Graphics.Drawables.Drawable;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.Media.ImageSource;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Controls.Platform
{
	partial class ImageSourceLoader
	{
		public static void LoadImage(IImageSource source, IMauiContext mauiContext, Action<IImageSourceServiceResult<NativeView>> finished = null)
		{
			LoadImageResult(source.GetNativeImage(mauiContext), finished)
						.FireAndForget(e => Internals.Log.Warning(nameof(ImageSourceLoader), $"{e}"));
		}

		static async Task LoadImageResult(Task<IImageSourceServiceResult<NativeView>> task, Action<IImageSourceServiceResult<NativeView>> finished = null)
		{
			var result = await task;
			finished?.Invoke(result);
		}
	}
}
