using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;
using UITest.Core;

namespace UITest.Appium
{
	// https://appium.github.io/appium-xcuitest-driver/5.0/capabilities/
	public class AppiumIOSApp : AppiumApp, IIOSApp
	{
		public AppiumIOSApp(Uri remoteAddress, IConfig config)
			: base(new IOSDriver(remoteAddress, GetOptions(config)), config)
		{
			_commandExecutor.AddCommandGroup(new AppiumIOSPointerActions(this));
			_commandExecutor.AddCommandGroup(new AppiumIOSVirtualKeyboardActions(this));
		}

		public override ApplicationState AppState
		{
			get
			{
				try
				{
					var appId = Config.GetProperty<string>("AppId") ?? throw new InvalidOperationException($"{nameof(AppState)} could not get the appid property");
					var state = _driver?.ExecuteScript("mobile: queryAppState", new Dictionary<string, object>
					{
						{ "bundleId", appId },
					});

					// https://developer.apple.com/documentation/xctest/xcuiapplicationstate?language=objc
					return Convert.ToInt32(state) switch
					{
						1 => ApplicationState.NotRunning,
						2 or
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
			config.SetProperty("PlatformName", "iOS");
			config.SetProperty("AutomationName", "XCUITest");

			var options = new AppiumOptions();
			SetGeneralAppiumOptions(config, options);

			var udid = config.GetProperty<string>("Udid");
			if (!string.IsNullOrWhiteSpace(udid))
			{
				options.AddAdditionalAppiumOption(MobileCapabilityType.Udid, udid);
			}

			var appId = config.GetProperty<string>("AppId");
			if (!string.IsNullOrWhiteSpace(appId))
			{
				options.AddAdditionalAppiumOption(IOSMobileCapabilityType.BundleId, appId);
			}

			return options;
		}
	}
}
