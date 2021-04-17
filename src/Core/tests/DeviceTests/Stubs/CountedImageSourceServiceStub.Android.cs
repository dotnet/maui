using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class CountedImageSourceServiceStub
	{
		public Task<Drawable> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource is ICountedImageSourceStub counted)
				return GetDrawableAsync(counted, context, cancellationToken);

			return Task.FromResult<Drawable>(null);
		}

		public async Task<Drawable> GetDrawableAsync(ICountedImageSourceStub imageSource, Context context, CancellationToken cancellationToken = default)
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

				return drawable;
			}
			finally
			{
				Finishing.Set();
			}
		}
	}
}