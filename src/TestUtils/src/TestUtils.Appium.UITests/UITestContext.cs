using Microsoft.Maui.Appium;
using Xamarin.UITest;

namespace TestUtils.Appium.UITests
{
	public sealed class UITestContext : IUITestContext
	{
		private bool _disposed;
		readonly IApp _app;
		readonly TestConfig _testConfig;

		public UITestContext(IApp app, TestConfig testConfig)
		{
			_app = app;
			_testConfig = testConfig;
		}

		public IApp App { get { return _disposed ? throw new ObjectDisposedException("Accessing IApp that has been disposed") : _app; } }
		public TestConfig TestConfig { get { return _testConfig; } }

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			if (_app is IApp2 app2)
			{
				app2.Dispose();
			}

			_disposed = true;
		}
	}
}