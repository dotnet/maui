using Microsoft.Maui.Appium;
using Xamarin.UITest;

namespace TestUtils.Appium.UITests
{
	public abstract class UITestContextTestBase
	{
		static IUITestContext? _uiTestContext;

		protected static IUITestContext UITestContext
		{
			get
			{
				return _uiTestContext ?? throw new InvalidOperationException("Call InitialSetup() before accessing the UITestContext");
			}
		}

		protected static IApp App { get { return UITestContext.App; } }

		public abstract TestConfig GetTestConfig();

		public void InitialSetup(IContext uiTestContext)
		{
			if (uiTestContext == null)
			{
				throw new ArgumentNullException(nameof(uiTestContext));
			}

			var testConfig = GetTestConfig();
			// Check to see if we have a context already from a previous test and if the configuration matches, re-use it
			if (_uiTestContext == null || !_uiTestContext.TestConfig.Equals(testConfig))
			{
				// Since the configuration doesn't match, we try to dispose of the old driver before creating a new one
				if (_uiTestContext != null && _uiTestContext.App is IApp2 oldApp2)
				{
					oldApp2.Dispose();
				}

				_uiTestContext = uiTestContext?.CreateUITestContext(testConfig); // Creating the driver can be expensive

				if (App is IApp2 app2)
				{
					// Mac is throwing if we call ActivateApp
					if (testConfig.TestDevice != TestDevice.Mac &&
						testConfig.TestDevice != TestDevice.Windows)
					{
						app2.ActivateApp();
					}
				}
			}

			if (_uiTestContext == null)
			{
				throw new InvalidOperationException("Failed to get the driver.");
			}
		}
	}
}
