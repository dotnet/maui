using UITest.Core;

namespace UITest.Appium.NUnit
{
	public abstract class UITestContextBase
	{
		static IUIClientContext? _uiTestContext;
		IServerContext? _context;
		protected TestDevice _testDevice;

		public UITestContextBase(TestDevice testDevice)
		{
			_testDevice = testDevice;
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

		public IApp RunningApp => App;

		public abstract IConfig GetTestConfig();

		public abstract void Close();

		public abstract void LaunchAppWithTest();

		public void InitialSetup(IServerContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));

			var existingContext = _uiTestContext;
			InitialSetup(context, false);
			if (existingContext is not null && ReferenceEquals(existingContext, _uiTestContext))
				LaunchAppWithTest();
		}

		public virtual void Reset()
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

			// WinAppDriver's launch command reuses immutable session capabilities, including appArguments.
			var recreateWindowsSession = _uiTestContext is not null && _testDevice == TestDevice.Windows
				&& GetStartupTest(_uiTestContext.Config) != GetStartupTest(testConfig);

			// Reuse mobile/Catalyst sessions; they accept a new startup target on each launch.
			if (reset || _uiTestContext == null || recreateWindowsSession)
			{
				var previousContext = _uiTestContext;
				_uiTestContext = null;
				previousContext?.Dispose();
				_uiTestContext = context.CreateUIClientContext(testConfig);
			}
			else
			{
				_uiTestContext.Config.SetProperty("TestConfigurationArgs",
					testConfig.GetProperty<Dictionary<string, string>>("TestConfigurationArgs"));
			}

			if (_uiTestContext == null)
			{
				throw new InvalidOperationException("Failed to get the driver.");
			}
		}

		static string? GetStartupTest(IConfig config)
		{
			var args = config.GetProperty<Dictionary<string, string>>("TestConfigurationArgs");
			return args is not null && args.TryGetValue("test", out var testName) ? testName : null;
		}
	}
}