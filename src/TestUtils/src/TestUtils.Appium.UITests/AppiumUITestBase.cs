using Microsoft.Maui.Appium;
using Xamarin.UITest;

namespace TestUtils.Appium.UITests
{
	public abstract class AppiumUITestBase : AppiumTestBase
	{
		IApp? _app;

		public IApp? App => _app;

		public void InitialSetup()
		{
			TestConfig = GetTestConfig();
			SetPlatformAppiumOptions(AppiumOptions);

			Driver = GetDriver();
			_app = new AppiumUITestApp(TestConfig.AppId, Driver);

			//Mac is throwing if we call ActivateApp
			if (TestConfig.TestDevice != TestDevice.Mac && TestConfig.TestDevice != TestDevice.Windows)
				Driver?.ActivateApp(TestConfig.AppId);

			//Driver?.LaunchApp();
		}

		public void Teardown()
		{
			Driver?.Quit();
			Server.Dispose();
		}
	}
}
