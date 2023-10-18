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
			_commandExecutor.AddCommandGroup(new AppiumAndroidVirtualKeyboardActions(this));
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
					0 => ApplicationState.Not_Installed,
					1 => ApplicationState.Not_Running,
					3 or
					4 => ApplicationState.Running,
					_ => ApplicationState.Unknown,
				};
			}
		}

		private static AppiumOptions GetOptions(IConfig config)
		{
			config.SetProperty("PlatformName", "Android");
			config.SetProperty("AutomationName", "UIAutomator2");

			var options = new AppiumOptions();
			SetGeneralAppiumOptions(config, options);

			var appId = config.GetProperty<string>("AppId");
			if (!string.IsNullOrWhiteSpace(appId))
			{
				options.AddAdditionalAppiumOption(IOSMobileCapabilityType.BundleId, appId);
			}

			return options;
		}
	}
}
