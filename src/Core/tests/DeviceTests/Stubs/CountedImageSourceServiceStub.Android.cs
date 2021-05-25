using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class CountedImageSourceServiceStub
	{
		public Task<IImageSourceServiceResult<Drawable>> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default) =>
			GetDrawableAsync((ICountedImageSourceStub)imageSource, context, cancellationToken);

		public async Task<IImageSourceServiceResult<Drawable>> GetDrawableAsync(ICountedImageSourceStub imageSource, Context context, CancellationToken cancellationToken = default)
		{
			try
			{
				Starting.Set();

				// simulate actual work
				var drawable = await Task.Run(() =>
				{
					if (imageSource.Wait)
						DoWork.WaitOne();

					var color = imageSource.Color.ToNative();

					return new ColorDrawable(color);
				}).ConfigureAwait(false);

				return new Result(drawable, imageSource.IsResolutionDependent);
			}
			finally
			{
				Finishing.Set();
			}
		}

		class Result : ImageSourceServiceResult
		{
			public Result(ColorDrawable drawable, bool resolutionDependent)
				: base(drawable, resolutionDependent, () => drawable.Dispose())
			{
			}
		}
	}
}