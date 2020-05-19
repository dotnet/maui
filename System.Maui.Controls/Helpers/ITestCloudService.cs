namespace System.Maui.Controls
{
	public interface ITestCloudService
	{
		bool IsOnTestCloud ();

		string GetTestCloudDeviceName ();

		string GetTestCloudDevice ();
	}
}

