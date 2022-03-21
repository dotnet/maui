using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Android.Graphics.Drawables;

namespace Microsoft.Maui
{
	internal class ImageLoaderResultCallback : Java.Lang.Object, IImageLoaderCallback
	{
		public ImageLoaderResultCallback(Action<Drawable?>? completeCallback = null)
		{
			CompleteCallback = completeCallback;
		}

		readonly TaskCompletionSource<IImageSourceServiceResult<Drawable>?> tcsResult = new();

		public Task<IImageSourceServiceResult<Drawable>?> Result
			=> tcsResult.Task;

		public readonly Action<Drawable?>? CompleteCallback;

		public void OnComplete(Java.Lang.Boolean? success, Drawable? drawable, Java.Lang.IRunnable? dispose)
		{
			try
			{
				var s = success?.BooleanValue() ?? false;

				Action? disposeWrapper = null;
				if (dispose != null)
				{
					disposeWrapper = () =>
					{
						dispose.Run();
					};
				}

				if (s && drawable is not null)
				{
					tcsResult.SetResult(new ImageSourceServiceResult(drawable!, disposeWrapper));
					return;
				}
			}
			catch
			{
			}

			tcsResult.SetResult(null);
		}
	}

	internal class ImageLoaderCallback : Java.Lang.Object, IImageLoaderCallback
	{
		public ImageLoaderCallback()
		{
		}

		readonly TaskCompletionSource<IImageSourceServiceResult?> tcsResult = new();

		public Task<IImageSourceServiceResult?> Result
			=> tcsResult.Task;

		public void OnComplete(Java.Lang.Boolean? success, Drawable? drawable, Java.Lang.IRunnable? dispose)
		{
			try
			{
				var s = success?.BooleanValue() ?? false;

				Action? disposeWrapper = null;
				if (dispose != null)
				{
					disposeWrapper = () =>
					{
						dispose.Run();
					};
				}

				if (s)
				{
					tcsResult.SetResult(new ImageSourceServiceLoadResult(disposeWrapper));
					return;
				}
			}
			catch
			{
			}

			tcsResult.SetResult(null);
		}
	}
}
