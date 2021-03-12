namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public interface ITestCloudService
	{
		bool IsOnTestCloud();

		string GetTestCloudDeviceName();

		string GetTestCloudDevice();
	}
}

