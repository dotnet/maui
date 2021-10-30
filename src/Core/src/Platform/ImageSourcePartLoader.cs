using System;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;

#if __IOS__ || MACCATALYST
using NativeImage = UIKit.UIImage;
using NativeView = UIKit.UIView;
#elif MONOANDROID
using NativeImage = Android.Graphics.Drawables.Drawable;
using NativeView = Android.Views.View;
#elif WINDOWS
using NativeImage = Microsoft.UI.Xaml.Media.ImageSource;
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
using NativeImage = System.Object;
#endif

namespace Microsoft.Maui
{
	public partial class ImageSourcePartLoader
	{
		IImageSourceServiceProvider? _imageSourceServiceProvider;
		IImageSourceServiceProvider ImageSourceServiceProvider =>
			_imageSourceServiceProvider ??= Handler.GetRequiredService<IImageSourceServiceProvider>();

		readonly Func<IImageSourcePart?> _imageSourcePart;
		Action<NativeImage?>? SetImage { get; }
		NativeView? NativeView => Handler.NativeView as NativeView;

		internal ImageSourceServiceResultManager SourceManager { get; } = new ImageSourceServiceResultManager();

		IElementHandler Handler { get; }

		public ImageSourcePartLoader(
			IElementHandler handler,
			Func<IImageSourcePart?> imageSourcePart,
			Action<NativeImage?> setImage)
		{
			Handler = handler;
			_imageSourcePart = imageSourcePart;
			SetImage = setImage;
		}

		public void Reset()
		{
			SourceManager.Reset();
		}

		public async Task UpdateImageSourceAsync()
		{
			if (NativeView != null)
			{
				var token = this.SourceManager.BeginLoad();
				var imageSource = _imageSourcePart();

				if (imageSource != null)
				{
#if __IOS__ || __ANDROID__ || WINDOWS
					var result = await imageSource.UpdateSourceAsync(NativeView, ImageSourceServiceProvider, SetImage!, token)
						.ConfigureAwait(false);

					SourceManager.CompleteLoad(result);
#else
					await Task.CompletedTask;
#endif
				}
				else
				{
					SetImage?.Invoke(null);
					SourceManager.CompleteLoad(null);
				}
			}
		}
	}
}
