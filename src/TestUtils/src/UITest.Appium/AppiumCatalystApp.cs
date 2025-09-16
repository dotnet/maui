using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.Mac;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumCatalystApp : AppiumApp, ICatalystApp
	{
		public AppiumCatalystApp(Uri remoteAddress, IConfig config)
			: base(new MacDriver(remoteAddress, GetOptions(config)), config)
		{
			_commandExecutor.AddCommandGroup(new AppiumAppleContextMenuActions(this));
			_commandExecutor.AddCommandGroup(new AppiumAppleStepperActions(this));
			_commandExecutor.AddCommandGroup(new AppiumCatalystMouseActions(this));
			_commandExecutor.AddCommandGroup(new AppiumCatalystTouchActions(this));
			_commandExecutor.AddCommandGroup(new AppiumCatalystAlertActions(this));
			_commandExecutor.AddCommandGroup(new AppiumCatalystSpecificActions(this));
			_commandExecutor.AddCommandGroup(new AppiumCatalystSwipeActions(this));
			_commandExecutor.AddCommandGroup(new AppiumCatalystThemeChangeAction());
			_commandExecutor.AddCommandGroup(new AppiumCatalystVirtualKeyboardActions(this));
			_commandExecutor.AddCommandGroup(new AppiumCatalystScrollActions(this));
		}

		public override ApplicationState AppState
		{
			get
			{
				try
				{
					var appId = Config.GetProperty<string>("AppId") ?? throw new InvalidOperationException($"{nameof(AppState)} could not get the appid property");
					var state = _driver?.ExecuteScript("macos: queryAppState", new Dictionary<string, object>
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
			config.SetProperty("PlatformName", "mac");
			config.SetProperty("AutomationName", "mac2");

			var options = new AppiumOptions();
			SetGeneralAppiumOptions(config, options);

			var appId = config.GetProperty<string>("AppId");
			if (!string.IsNullOrWhiteSpace(appId))
			{
				options.AddAdditionalAppiumOption(IOSMobileCapabilityType.BundleId, appId);
			}

			var args = config.GetProperty<Dictionary<string, string>>("TestConfigurationArgs");
			if(args is not null && args.Count > 0)
			{
				options.AddAdditionalAppiumOption("environment", args);
			}

			options.AddAdditionalAppiumOption("showServerLogs", true);
			return options;
		}
	}
}
