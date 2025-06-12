using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.ApplicationModel;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class CountedImageSourceServiceStub
	{
		public Task<IImageSourceServiceResult<UIImage>> GetImageAsync(IImageSource imageSource, float scale = 1, CancellationToken cancellationToken = default) =>
			GetImageAsync((ICountedImageSourceStub)imageSource, scale, cancellationToken);

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

					var color = imageSource.Color.ToPlatform();

					return CreateImageAsync(scale, color);
				}).ConfigureAwait(false);

				return new Result(image, imageSource.IsResolutionDependent);
			}
			finally
			{
				Finishing.Set();
			}
		}


		static async Task<UIImage> CreateImageAsync(float scale, UIColor color)
		{
			var rect = new CGRect(0, 0, 100, 100);

			return await MainThread.InvokeOnMainThreadAsync(() =>
			{
				var renderer = new UIGraphicsImageRenderer(rect.Size, new UIGraphicsImageRendererFormat()
				{
					Opaque = false,
					Scale = scale,
				});

				var image = renderer.CreateImage((context) =>
				{
					color.SetFill();
					context.FillRect(rect);
				}).ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

				return image;
			});
		}

		class Result : ImageSourceServiceResult
		{
			public Result(UIImage image, bool resolutionDependent)
				: base(image, resolutionDependent, () => image.Dispose())
			{
			}
		}
	}
}