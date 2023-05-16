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
#if IOS || ANDROID || WINDOWS || TIZEN
		IImageSourceServiceProvider? _imageSourceServiceProvider;
#endif

		readonly WeakReference<IImageSourcePartSetter> _handler;

		internal ImageSourceServiceResultManager SourceManager { get; } = new ImageSourceServiceResultManager();

		[Obsolete("Use ImageSourcePartLoader(IImageSourcePartSetter handler) instead.")]
		public ImageSourcePartLoader(IElementHandler handler, Func<IImageSourcePart?> imageSourcePart, Action<PlatformImage?> setImage)
			: this((IImageSourcePartSetter)handler)
		{
		}

		public ImageSourcePartLoader(IImageSourcePartSetter handler) => _handler = new(handler);

		public void Reset()
		{
			SourceManager.Reset();
		}

		public async Task UpdateImageSourceAsync()
		{
			if (!_handler.TryGetTarget(out var handler) || handler.PlatformView is not PlatformView platformView)
			{
				return;
			}

			var token = this.SourceManager.BeginLoad();
			var imageSource = handler.VirtualView as IImageSourcePart;

			if (imageSource?.Source is not null)
			{
#if IOS || ANDROID || WINDOWS || TIZEN
				_imageSourceServiceProvider ??= handler.GetRequiredService<IImageSourceServiceProvider>();

				var result = await imageSource.UpdateSourceAsync(platformView, _imageSourceServiceProvider, handler.SetImageSource, token)
					.ConfigureAwait(false);

				SourceManager.CompleteLoad(result);
#else
				await Task.CompletedTask;
#endif
			}
			else
			{
				handler.SetImageSource(null);
			}
		}
	}
}
