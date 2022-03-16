using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class CountedImageSourceServiceStub
	{
		public async Task<IImageSourceServiceResult<bool>> LoadDrawableAsync(IImageSource imageSource, Android.Widget.ImageView imageView, CancellationToken cancellationToken = default)
		{
			if (imageSource is not ICountedImageSourceStub imageSourceStub)
				return new Result(false, false, null);

			try
			{
				Starting.Set();

				// simulate actual work
				var drawable = await Task.Run(() =>
				{
					if (imageSourceStub.Wait)
						DoWork.WaitOne();

					var color = imageSourceStub.Color.ToPlatform();

					return new ColorDrawable(color);
				}).ConfigureAwait(false);

				imageView.SetImageDrawable(drawable);

				return new Result(drawable is not null, imageSourceStub.IsResolutionDependent, drawable);
			}
			finally
			{
				Finishing.Set();
			}
		}

		public async Task<IImageSourceServiceResult<bool>> LoadDrawableAsync(Context context, IImageSource imageSource, Action<Drawable> callback, CancellationToken cancellationToken = default)
		{
			if (imageSource is not ICountedImageSourceStub imageSourceStub)
				return new Result(false, false, null);

			try
			{
				Starting.Set();

				// simulate actual work
				var drawable = await Task.Run(() =>
				{
					if (imageSourceStub.Wait)
						DoWork.WaitOne();

					var color = imageSourceStub.Color.ToPlatform();

					return new ColorDrawable(color);
				}).ConfigureAwait(false);

				return new Result(drawable is not null, imageSourceStub.IsResolutionDependent, drawable);
			}
			finally
			{
				Finishing.Set();
			}
		}

		class Result : ImageSourceServiceResult
		{
			public Result(bool success, bool resolutionDependent, Drawable drawable)
				: base(success, resolutionDependent, () => drawable?.Dispose())
			{
			}
		}
	}
}