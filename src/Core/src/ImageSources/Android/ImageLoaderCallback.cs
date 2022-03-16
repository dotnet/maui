using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Android.Graphics.Drawables;

namespace Microsoft.Maui
{
	internal class ImageLoaderCallback : Java.Lang.Object, IImageLoaderCallback
	{
		TaskCompletionSource<IImageSourceServiceResult<bool>> tcsResult = new();

		public Task<IImageSourceServiceResult<bool>> Result
			=> tcsResult.Task;

		public void OnComplete(Java.Lang.Boolean? success, Java.Lang.IRunnable? dispose)
		{
			var s = success?.BooleanValue() ?? false;

			Action? disposeWrapper = null;
			if (dispose != null)
				disposeWrapper = dispose.Run;

			tcsResult.TrySetResult(new ImageSourceServiceResult(s, disposeWrapper));
		}
	}

	internal class ImageLoaderDrawableCallback : Java.Lang.Object, IImageLoaderDrawableCallback
	{
		public ImageLoaderDrawableCallback(Action<Drawable?> handler)
		{
			_handler = handler;
		}

		readonly Action<Drawable?> _handler;

		TaskCompletionSource<IImageSourceServiceResult<bool>> tcsResult = new();

		public Task<IImageSourceServiceResult<bool>> Result
			=> tcsResult.Task;

		public void OnComplete(Drawable? drawable, Java.Lang.IRunnable? dispose)
		{
			try
			{
				_handler?.Invoke(drawable);

				Action? disposeWrapper = null;
				if (dispose != null)
					disposeWrapper = dispose.Run;

				tcsResult.TrySetResult(new ImageSourceServiceResult(drawable is not null, disposeWrapper));
			}
			catch
			{
				tcsResult.TrySetResult(new ImageSourceServiceResult(false));
			}
		}
	}
}
