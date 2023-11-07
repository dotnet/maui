using UITest.Core;

namespace UITest.Appium.NUnit
{
	public abstract class UITestContextBase
	{
		static IUIClientContext? _uiTestContext;
		IServerContext? _context;
		protected TestDevice _testDevice;
		protected bool _useBrowserStack;

		public UITestContextBase(TestDevice testDevice, bool useBrowserStack)
		{
			_testDevice = testDevice;
			_useBrowserStack = useBrowserStack;
		}

		public static IUIClientContext? UITestContext { get { return _uiTestContext; } }

		public TestDevice Device
		{
			get
			{
				return UITestContext == null
					? throw new InvalidOperationException($"Call {nameof(InitialSetup)} before accessing the {nameof(Device)} property.")
					: UITestContext.Config.GetProperty<TestDevice>("TestDevice");
			}
		}

		public IApp App
		{
			get
			{
				return UITestContext == null
					? throw new InvalidOperationException($"Call {nameof(InitialSetup)} before accessing the {nameof(App)} property.")
					: UITestContext.App;
			}
		}

		/// <summary>
		/// If true, the app will be restarted for each test. In that case, there's no need, for instance,
		/// for FixtureTeardown to reset the app state, going back to the home screen.
		/// </summary>
		public bool RunTestsInIsolation => _useBrowserStack;

		public abstract IConfig GetTestConfig();

		public void InitialSetup(IServerContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			InitialSetup(context, false);
		}

		public void Reset()
		{
			if (_context == null)
			{
				throw new InvalidOperationException($"Cannot {nameof(Reset)} if {nameof(InitialSetup)} has not been called.");
			}

			InitialSetup(_context, true);
		}

		private void InitialSetup(IServerContext context, bool reset)
		{
			var testConfig = GetTestConfig();
			testConfig.SetProperty("TestDevice", _testDevice);
			testConfig.SetProperty("UseBrowserStack", _useBrowserStack);

			// Check to see if we have a context already from a previous test and re-use it as creating the driver is expensive
			if (reset || _uiTestContext == null)
			{
				_uiTestContext?.Dispose();
				_uiTestContext = context.CreateUIClientContext(testConfig);
			}

			if (_uiTestContext == null)
			{
				throw new InvalidOperationException("Failed to get the driver.");
			}
		}

		public bool IsSetup => _uiTestContext != null;

		// Dispose of the driver. This is currently only used for BrowserStack, where the driver is recreated for each test
		// so each test has its own session.
		public void TearDown()
		{
			_uiTestContext?.Dispose();
			_uiTestContext = null;
		}
	}
}