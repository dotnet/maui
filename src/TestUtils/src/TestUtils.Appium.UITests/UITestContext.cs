using Microsoft.Maui.Appium;
using Xamarin.UITest;

namespace TestUtils.Appium.UITests
{
	public class UITestContext : IUITestContext
	{
		readonly IApp _app;
		readonly TestConfig _testConfig;

		public UITestContext(IApp app, TestConfig testConfig)
		{
			_app = app;
			_testConfig = testConfig;
		}

		public IApp App { get { return _app; } }
		public TestConfig TestConfig { get { return _testConfig; } }

		public void Dispose()
		{
			if (_app is IApp2 app2)
			{
				app2.Dispose();
			}
		}
	}
}