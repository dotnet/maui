namespace Xamarin.Forms.Controls
{
	public interface ITestCloudService
	{
		bool IsOnTestCloud ();

		string GetTestCloudDeviceName ();

		string GetTestCloudDevice ();
	}
}

