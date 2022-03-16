using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Android.Graphics.Drawables;

namespace Microsoft.Maui
{
	internal class ImageLoaderCallback : Java.Lang.Object, IImageLoaderCallback
	{
		public ImageLoaderCallback(Action<Drawable?>? handler = default)
		{
			_handler = handler;
		}

		readonly Action<Drawable?>? _handler;

		readonly TaskCompletionSource<IImageSourceServiceResult<bool>> tcsResult = new();

		public Task<IImageSourceServiceResult<bool>> Result
			=> tcsResult.Task;

		public void OnComplete(Java.Lang.Boolean? success, Drawable? drawable, Java.Lang.IRunnable? dispose)
		{
			try
			{
				var s = success?.BooleanValue() ?? false;

				_handler?.Invoke(drawable);

				Action? disposeWrapper = null;
				if (dispose != null)
					disposeWrapper = dispose.Run;

				tcsResult.SetResult(new ImageSourceServiceResult(s, disposeWrapper));
			}
			catch
			{
				tcsResult.SetResult(new ImageSourceServiceResult(false));
			}
		}
	}
}
