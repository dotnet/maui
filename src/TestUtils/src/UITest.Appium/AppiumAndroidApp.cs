using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumAndroidApp : AppiumApp, IAndroidApp
	{
		private AppiumAndroidApp(Uri remoteAddress, IConfig config)
			: base(new AndroidDriver(remoteAddress, GetOptions(config)), config)
		{
			_commandExecutor.AddCommandGroup(new AppiumAndroidContextMenuActions(this));
			_commandExecutor.AddCommandGroup(new AppiumAndroidThemeChangeAction());
			_commandExecutor.AddCommandGroup(new AppiumAndroidSpecificActions(this));
			_commandExecutor.AddCommandGroup(new AppiumAndroidVirtualKeyboardActions(this));
			_commandExecutor.AddCommandGroup(new AppiumAndroidAlertActions(this));
			_commandExecutor.AddCommandGroup(new AppiumAndroidStepperActions(this));
		}

		public static AppiumAndroidApp CreateAndroidApp(Uri remoteAddress, IConfig config)
		{
			var device = config.GetProperty<string>("EmulatorDeviceName");
			var apkPath = config.GetProperty<string>("AppPath");
			var pkgName = config.GetProperty<string>("AppId");
			var outDir = config.GetProperty<string>("ReportDirectory");
			var enableDebugPopup = config.GetProperty<bool>("EnableDebugPopup");
			var avdForce = config.GetProperty<bool>("AvdForceInstall");

			if (enableDebugPopup)
				Environment.SetEnvironmentVariable("SWIFTSHADER_DISABLE_DEBUGGER_WAIT_DIALOG", "0");
			else
				Environment.SetEnvironmentVariable("SWIFTSHADER_DISABLE_DEBUGGER_WAIT_DIALOG", "1");

			//// Will check if AVD with device name already exists first and not reinstall
			//AndroidEmulator.AvdCreate(device, force: avdForce);
			//// StartEmulator will return immediately if emulator is already running
			//AndroidEmulator.StartEmulator(device);
			//// TODO: Check for installed package first?
			//AndroidEmulator.InstallPackage(apkPath, pkgName, outDir);
			var androidApp = new AppiumAndroidApp(remoteAddress, config);
			androidApp.Driver.ActivateApp(pkgName);
			return androidApp;
		}

		public override IUIElementQueryable Query => new AppiumAndroidQueryable(this);

		public override ApplicationState AppState
		{
			get
			{
				try
				{
					var appId = Config.GetProperty<string>("AppId") ?? throw new InvalidOperationException($"{nameof(AppState)} could not get the appid property");
					var state = _driver?.ExecuteScript("mobile: queryAppState", new Dictionary<string, object>
					{
						{ "appId", appId },
					});

					// https://github.com/appium/appium-uiautomator2-driver#mobile-queryappstate
					if (state == null)
					{
						return ApplicationState.Unknown;
					}

					return Convert.ToInt32(state) switch
					{
						0 => ApplicationState.NotInstalled,
						1 => ApplicationState.NotRunning,
						3 or
						4 => ApplicationState.Running,
						_ => ApplicationState.Unknown,
					};
				}
				catch
				{
					return ApplicationState.Unknown;
				}
			}
		}

		private static AppiumOptions GetOptions(IConfig config)
		{
			config.SetProperty("PlatformName", "Android");
			config.SetProperty("AutomationName", "UIAutomator2");
			var appId = config.GetProperty<string>("AppId");

			var options = new AppiumOptions();

			SetGeneralAppiumOptions(config, options);

			if (!string.IsNullOrWhiteSpace(appId))
			{
				options.AddAdditionalAppiumOption(MobileCapabilityType.NoReset, "true");
				options.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppPackage, appId);
				options.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppActivity, $"{appId}.MainActivity");
			}
			
			var headless = config.GetProperty<bool>("Headless");
			if (headless)
			{
				// Appium has the ability to start Android emulators in a "headless" mode.
				// This means that the devices won't have any graphical user interface; but they will still be running silently, testing the app.
				options.AddAdditionalAppiumOption("isHeadless", true);
			}

			// The UiAutomator2 driver continuously checks for toast messages, consuming valuable CPU cycles.
			// Because our tests don’t require this feature, disable it to improve performance and consistency, especially in scrolling actions.
			options.AddAdditionalAppiumOption("disableAndroidWatchers", "true");

			// Maximum amount of milliseconds to wait until the application under test is installed.
			// 90000 ms by default
			options.AddAdditionalAppiumOption("appium:androidInstallTimeout", 60000);

			// Timeout used for waiting for the user interface to go into an idle state.
			// By default, all core uiautomator objects except UiDevice will perform this wait before starting to search for the widget specified by the object's locator.
			// Once the idle state is detected or the timeout elapses (whichever occurs first), the object will start to wait for the selector to find a match. 
			// 10000 ms by default.
			options.AddAdditionalAppiumOption("appium:waitForIdleTimeout", 5000);

			// Whether to disable window animations when starting the instrumentation process.
			// The animation scale will be restored automatically after the instrumentation process ends.
			options.AddAdditionalAppiumOption("appium:disableWindowAnimation", true);

			return options;
		}
	}
}
