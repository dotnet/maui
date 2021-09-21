using System;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
#if __IOS__ || MACCATALYST
using NativeImage = UIKit.UIImage;
#elif MONOANDROID
using NativeImage = Android.Graphics.Drawables.Drawable;
#elif WINDOWS
using NativeImage = Microsoft.UI.Xaml.Media.ImageSource;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeImage = System.Object;
#endif

namespace Microsoft.Maui
{
	public partial class ImageSourcePartLoader
	{
		readonly Func<IImageSourcePart?> _imageSourcePart;

		public ImageSourceServiceResultManager SourceManager { get; } = new ImageSourceServiceResultManager();

		IElementHandler Handler { get; }

		internal ImageSourcePartLoader(
			IElementHandler handler,
			Func<IImageSourcePart?> imageSourcePart)
		{
			Handler = handler;
			_imageSourcePart = imageSourcePart;
		}

		public void Reset()
		{
			SourceManager.Reset();
		}

		public async Task UpdateImageSourceAsync()
		{
#if __IOS__ || __ANDROID__ || WINDOWS
			if (NativeView != null)
			{
				var token = this.SourceManager.BeginLoad();
				var provider = Handler.GetRequiredService<IImageSourceServiceProvider>();
				var imageSource = _imageSourcePart();

				if (imageSource != null)
				{
					var result = await imageSource.UpdateSourceAsync(NativeView, provider, SetImage!, token);
					SourceManager.CompleteLoad(result);
				}
				else
				{
					SetImage?.Invoke(null);
					SourceManager.CompleteLoad(null);
				}
			}
#else
			await Task.CompletedTask;
#endif
		}
	}
}
