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

			// Activate the app to bring it to foreground
			try
			{
				var appId = config.GetProperty<string>("AppId");
				if (!string.IsNullOrWhiteSpace(appId))
				{
					_driver?.ExecuteScript("macos: activateApp", new Dictionary<string, object>
					{
						{ "bundleId", appId }
					});
				}
			}
			catch (Exception)
			{
				// Ignore activation errors - app may already be active
			}
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

		// Override to avoid XPath queries which can fail on mac2 driver.
		// On Mac Catalyst, AutomationId maps to accessibilityIdentifier.
		// NOTE: Window-scoped searches were removed because they cause StaleElementReferenceException
		// when window elements expire from Appium's cache. Driver-level search works reliably.

		public override IUIElement FindElement(string id)
		{
			// Use driver-level search - window-scoped search causes StaleElementReferenceException
			var byAccessibility = AppiumQuery.ByAccessibilityId(id).FindElement(this);
			if (byAccessibility != null)
				return byAccessibility;

			return Query.ById(id).FirstOrDefault()!;
		}

		public override IReadOnlyCollection<IUIElement> FindElements(string id)
		{
			// Use driver-level search - window-scoped search causes StaleElementReferenceException
			var byAccessibility = AppiumQuery.ByAccessibilityId(id).FindElements(this);
			if (byAccessibility != null && byAccessibility.Count > 0)
				return byAccessibility;

			return Query.ById(id);
		}

		public override IUIElement FindElementByText(string text)
		{
			// Use driver-level search - window-scoped search causes StaleElementReferenceException
			var byAccessibility = AppiumQuery.ByAccessibilityId(text).FindElement(this);
			if (byAccessibility != null)
				return byAccessibility;

			var byName = AppiumQuery.ByName(text).FindElement(this);
			if (byName != null)
				return byName;

			return null!;
		}

		public override IReadOnlyCollection<IUIElement> FindElementsByText(string text)
		{
			// Use driver-level search - window-scoped search causes StaleElementReferenceException
			var byAccessibility = AppiumQuery.ByAccessibilityId(text).FindElements(this);
			if (byAccessibility != null && byAccessibility.Count > 0)
				return byAccessibility;

			var byName = AppiumQuery.ByName(text).FindElements(this);
			if (byName != null && byName.Count > 0)
				return byName;

			return new List<IUIElement>();
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
