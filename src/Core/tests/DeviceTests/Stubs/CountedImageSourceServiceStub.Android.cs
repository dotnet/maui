using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class CountedImageSourceServiceStub
	{
		public async Task<IImageSourceServiceResult> LoadDrawableAsync(IImageSource imageSource, global::Android.Widget.ImageView imageView, CancellationToken cancellationToken = default)
		{
			if (imageSource is not ICountedImageSourceStub imageSourceStub)
				return null;

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

				cancellationToken.ThrowIfCancellationRequested();
				imageView.SetImageDrawable(drawable);

				return new ImageSourceServiceLoadResult(drawable.Dispose);
			}
			finally
			{
				Finishing.Set();
			}
		}

		public async Task<IImageSourceServiceResult<Drawable>> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			if (imageSource is not ICountedImageSourceStub imageSourceStub)
				return null;

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

				return new ImageSourceServiceResult(drawable, imageSourceStub.IsResolutionDependent, drawable.Dispose);
			}
			finally
			{
				Finishing.Set();
			}
		}
	}
}