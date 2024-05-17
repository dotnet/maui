using System;
using Microsoft.Maui.Controls.ControlGallery;

namespace Microsoft.Maui.Controls.ControlGallery.Android
{
	public class TestCloudService : ITestCloudService
	{
		public bool IsOnTestCloud()
		{
			var isInTestCloud = System.Environment.GetEnvironmentVariable("XAMARIN_TEST_CLOUD");

			return isInTestCloud != null && isInTestCloud.Equals("1", StringComparison.Ordinal);
		}

		public string GetTestCloudDeviceName()
		{
			return System.Environment.GetEnvironmentVariable("XTC_DEVICE_NAME");
		}

		public string GetTestCloudDevice()
		{
			return System.Environment.GetEnvironmentVariable("XTC_DEVICE");
		}
	}
}