using System.IO;
using System.Threading.Tasks;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Media
{
	partial class ScreenshotImplementation : IScreenshot
	{
		public bool PlatformIsCaptureSupported =>
			UIScreen.MainScreen != null;

		public Task<IScreenshotResult> CaptureAsync()
		{
			var img = UIScreen.MainScreen.Capture();
			var result = new ScreenshotResult(img);

			return Task.FromResult(result);
		}
	}

	partial class ScreenshotResult
	{
		readonly UIImage uiImage;

		internal ScreenshotResult(UIImage image)
		{
			uiImage = image;

			Width = (int)(image.Size.Width * image.CurrentScale);
			Height = (int)(image.Size.Height * image.CurrentScale);
		}

		internal Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format)
		{
			var data = format switch
			{
				ScreenshotFormat.Jpeg => uiImage.AsJPEG(),
				_ => uiImage.AsPNG()
			};

			return Task.FromResult(data.AsStream());
		}
	}
}
