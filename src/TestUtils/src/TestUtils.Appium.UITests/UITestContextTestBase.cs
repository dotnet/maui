using Microsoft.Maui.Appium;
using Xamarin.UITest;

namespace TestUtils.Appium.UITests
{
	public abstract class UITestContextTestBase
	{
		static IUITestContext? _uiTestContext;
		private IContext? _context;

		protected static IUITestContext? UITestContext
		{
			get
			{
				return _uiTestContext;
			}
		}

		protected static TestDevice Device
		{
			get
			{
				return UITestContext == null
					? throw new InvalidOperationException($"Call {nameof(InitialSetup)} before accessing the {nameof(Device)} property.")
					: UITestContext.TestConfig.TestDevice;
			}
		}

		protected static IApp App
		{
			get
			{
				return UITestContext == null
					? throw new InvalidOperationException($"Call {nameof(InitialSetup)} before accessing the {nameof(App)} property.")
					: UITestContext.App;
			}
		}

		public abstract TestConfig GetTestConfig();

		public void Reset()
		{
			if (_context == null)
			{
				throw new InvalidOperationException($"Cannot {nameof(Reset)} if {nameof(InitialSetup)} has not been called.");
			}

			InitialSetup(_context, true);
		}

		public void InitialSetup(IContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			InitialSetup(context, false);
		}

		private void InitialSetup(IContext context, bool reset)
		{
			var testConfig = GetTestConfig();
			// Check to see if we have a context already from a previous test and if the configuration matches, re-use it
			if (reset || _uiTestContext == null || !_uiTestContext.TestConfig.Equals(testConfig))
			{
				_uiTestContext?.Dispose();
				_uiTestContext = context.CreateUITestContext(testConfig); // Creating the driver can be expensive

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
