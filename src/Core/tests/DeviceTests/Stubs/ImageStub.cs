using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class ImageStub : StubBase, IImage, IImageSourcePartEvents
	{
		public Aspect Aspect { get; set; }

		public bool IsOpaque { get; set; }

		public IImageSource Source { get; set; }

		public bool IsAnimationPlaying { get; set; }

		public bool IsLoading { get; private set; }

		public event Action LoadingStarted;
		public event Action<bool> LoadingCompleted;
		public event Action<Exception> LoadingFailed;

		public void UpdateIsLoading(bool isLoading) =>
			IsLoading = isLoading;

		void IImageSourcePartEvents.LoadingCompleted(bool successful) =>
			LoadingCompleted?.Invoke(successful);

		void IImageSourcePartEvents.LoadingFailed(Exception exception) =>
			LoadingFailed?.Invoke(exception);

		void IImageSourcePartEvents.LoadingStarted() =>
			LoadingStarted?.Invoke();
	}

	public static class ImageStubExtensions
	{
		static readonly Random rnd = new Random();

		public static async Task Wait(this ImageStub image, int timeout = 1000)
		{
			while ((timeout -= 100) > 0)
			{
				if (image.IsLoading)
					await Task.Delay(rnd.Next(100, 200));
				else
					break;
			}
		}
	}
}