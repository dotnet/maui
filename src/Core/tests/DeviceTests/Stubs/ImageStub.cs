using System.Threading.Tasks;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class ImageStub : StubBase, IImage
	{
		public Aspect Aspect { get; set; }

		public bool IsOpaque { get; set; }

		public IImageSource Source { get; set; }

		public bool IsAnimationPlaying { get; set; }

		public bool IsLoading { get; private set; }

		public void UpdateIsLoading(bool isLoading)
		{
			IsLoading = isLoading;
		}
	}

	public static class ImageStubExtensions
	{
		public static async Task Wait(this ImageStub image, int timeout = 1000)
		{
			while ((timeout -= 100) > 0)
			{
				if (image.IsLoading)
					await Task.Delay(100);
				else
					break;
			}
		}
	}
}