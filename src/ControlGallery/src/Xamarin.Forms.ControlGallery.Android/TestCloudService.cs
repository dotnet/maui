using Xamarin.Forms.Controls;

namespace Xamarin.Forms.ControlGallery.Android
{
	public class TestCloudService : ITestCloudService
	{
		public bool IsOnTestCloud ()
		{
			var isInTestCloud = System.Environment.GetEnvironmentVariable ("XAMARIN_TEST_CLOUD");

			return isInTestCloud != null && isInTestCloud.Equals ("1");
		}

		public string GetTestCloudDeviceName ()
		{
			return System.Environment.GetEnvironmentVariable ("XTC_DEVICE_NAME");
		}

		public string GetTestCloudDevice ()
		{
			return System.Environment.GetEnvironmentVariable ("XTC_DEVICE");
		}
	}
}