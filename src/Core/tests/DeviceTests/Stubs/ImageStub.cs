using System;
using System.Threading.Tasks;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class ImageStub : StubBase, IImageStub, IImageSourcePartEvents
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

		void IImageSourcePartEvents.LoadingCompleted(bool successful)
		{
			IsLoading = false;
			LoadingCompleted?.Invoke(successful);
		}

		void IImageSourcePartEvents.LoadingFailed(Exception exception)
		{
			IsLoading = false;
			LoadingFailed?.Invoke(exception);
		}

		void IImageSourcePartEvents.LoadingStarted()
		{
			IsLoading = true;
			LoadingStarted?.Invoke();
		}
	}

	public static class ImageStubExtensions
	{
		public static Task WaitUntilLoaded(this IImageStub image, int timeout = 1000) =>
			AssertEventually(() => !image.IsLoading, timeout: timeout, message: $"Image {image} did not load before timeout");
	}
}