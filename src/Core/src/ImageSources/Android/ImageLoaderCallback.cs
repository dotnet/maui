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
		// Glide does not support starting new loads from Target callbacks.
		//
		// > You can't start or clear loads in RequestListener or Target callbacks.
		// > If you're trying to start a fallback request when a load fails, use RequestBuilder#error(RequestBuilder).
		// > Otherwise consider posting your into() or clear() calls to the main thread using a Handler instead.
		//
		// For this reason, if someone awaits for the result of the image loading, to start a new one
		// we must ensure the new load happens asynchronously.
		// This use case is covered by `LoadDrawableAsyncReturnsWithSameImageAndDoesNotHang` core device test.
		readonly TaskCompletionSource<T?> _tcsResult = new(TaskCreationOptions.RunContinuationsAsynchronously);

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
