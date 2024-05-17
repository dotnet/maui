namespace Microsoft.Maui.Controls.ControlGallery
{
	public interface ITestCloudService
	{
		bool IsOnTestCloud();

		string GetTestCloudDeviceName();

		string GetTestCloudDevice();
	}
}

