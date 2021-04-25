using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class CountedImageSourceServiceStub
	{
		public Task<IImageSourceServiceResult<UIImage>> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			if (imageSource is ICountedImageSourceStub counted)
				return GetImageAsync(counted, scale, cancellationToken);

			return Task.FromResult<IImageSourceServiceResult<UIImage>>(null);
		}

		public async Task<IImageSourceServiceResult<UIImage>> GetImageAsync(ICountedImageSourceStub imageSource, float scale = 1, CancellationToken cancellationToken = default)
		{
			try
			{
				Starting.Set();

				// simulate actual work
				var image = await Task.Run(() =>
				{
					if (imageSource.Wait)
						DoWork.WaitOne();

					var color = imageSource.Color.ToNative();

					return CreateImage(scale, color);
				}).ConfigureAwait(false);

				return new Result(image);
			}
			finally
			{
				Finishing.Set();
			}
		}

		static UIImage CreateImage(float scale, UIColor color)
		{
			var rect = new CGRect(0, 0, 100, 100);

			UIGraphics.BeginImageContextWithOptions(rect.Size, false, scale);
			var context = UIGraphics.GetCurrentContext();

			color.SetFill();
			context.FillRect(rect);

			var image = UIGraphics.GetImageFromCurrentImageContext();

			UIGraphics.EndImageContext();

			return image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
		}

		class Result : IImageSourceServiceResult<UIImage>
		{
			public Result(UIImage drawable)
			{
				Value = drawable;
			}

			public UIImage Value { get; }

			public void Dispose()
			{
				Value.Dispose();
			}
		}
	}
}