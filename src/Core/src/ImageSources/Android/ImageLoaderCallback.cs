using System;
using System.Threading.Tasks;
using Android.Graphics.Drawables;

namespace Microsoft.Maui
{
	class ImageLoaderResultCallback : ImageLoaderCallbackBase<IImageSourceServiceResult<Drawable>>
	{
		protected override IImageSourceServiceResult<Drawable>? OnSuccess(Drawable? drawable, Action? dispose) =>
			drawable is not null
				? new ImageSourceServiceResult(drawable, dispose)
				: default;
	}

	class ImageLoaderCallback : ImageLoaderCallbackBase<IImageSourceServiceResult>
	{
		protected override IImageSourceServiceResult? OnSuccess(Drawable? drawable, Action? dispose) =>
			new ImageSourceServiceLoadResult(dispose);
	}

	abstract class ImageLoaderCallbackBase<T> : Java.Lang.Object, IImageLoaderCallback
		where T : IImageSourceServiceResult
	{
		readonly TaskCompletionSource<T?> _tcsResult = new();

		public Task<T?> Result => _tcsResult.Task;

		public void OnComplete(Java.Lang.Boolean? success, Drawable? drawable, Java.Lang.IRunnable? dispose)
		{
			try
			{
				Action? disposeWrapper = dispose != null
					? dispose.Run
					: null;

				var result = success?.BooleanValue() == true
					? OnSuccess(drawable, disposeWrapper)
					: OnFailure(drawable, disposeWrapper);

				_tcsResult.SetResult(result);
			}
			catch (Exception ex)
			{
				_tcsResult.SetException(ex);
			}
		}

		protected abstract T? OnSuccess(Drawable? drawable, Action? dispose);

		protected virtual T? OnFailure(Drawable? errorDrawable, Action? dispose) => default;
	}
}
