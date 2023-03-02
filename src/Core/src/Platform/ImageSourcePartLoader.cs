using System;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;

#if IOS || MACCATALYST
using PlatformImage = UIKit.UIImage;
using PlatformView = UIKit.UIView;
#elif ANDROID
using PlatformImage = Android.Graphics.Drawables.Drawable;
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformImage = Microsoft.UI.Xaml.Media.ImageSource;
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformImage = Microsoft.Maui.Platform.MauiImageSource;
using PlatformView = Tizen.NUI.BaseComponents.View;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformImage = System.Object;
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Platform
{
	public partial class ImageSourcePartLoader
	{
		IImageSourceServiceProvider? _imageSourceServiceProvider;
		IImageSourceServiceProvider ImageSourceServiceProvider =>
			_imageSourceServiceProvider ??= Handler.GetRequiredService<IImageSourceServiceProvider>();

		readonly Func<IImageSourcePart?> _imageSourcePart;
		Action<PlatformImage?>? SetImage { get; }
		PlatformView? PlatformView => Handler.PlatformView as PlatformView;

		internal ImageSourceServiceResultManager SourceManager { get; } = new ImageSourceServiceResultManager();

		IElementHandler Handler { get; }

		public ImageSourcePartLoader(
			IElementHandler handler,
			Func<IImageSourcePart?> imageSourcePart,
			Action<PlatformImage?> setImage)
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
			if (PlatformView is null)
			{
				return;
			}

			var token = this.SourceManager.BeginLoad();
			var imageSource = _imageSourcePart();

			if (imageSource?.Source is not null)
			{
#if __IOS__ || __ANDROID__ || WINDOWS || TIZEN
				var result = await imageSource.UpdateSourceAsync(PlatformView, ImageSourceServiceProvider, SetImage!, token)
					.ConfigureAwait(false);

				SourceManager.CompleteLoad(result);
#else
				await Task.CompletedTask;
#endif
			}
			else
			{
				SetImage?.Invoke(null);
			}
		}
	}
}
