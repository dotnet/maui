using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class CountedImageSourceServiceStub
	{
		public Task<IImageSourceServiceResult<Drawable>> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource is ICountedImageSourceStub counted)
				return GetDrawableAsync(counted, context, cancellationToken);

			return Task.FromResult<IImageSourceServiceResult<Drawable>>(null);
		}

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

				return new Result(drawable);
			}
			finally
			{
				Finishing.Set();
			}
		}

		class Result : IImageSourceServiceResult<Drawable>
		{
			public Result(ColorDrawable drawable)
			{
				Value = drawable;
			}

			public Drawable Value { get; }

			public void Dispose()
			{
				Value.Dispose();
			}
		}
	}
}