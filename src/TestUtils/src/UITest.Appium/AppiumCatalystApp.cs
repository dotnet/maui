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

			// For multi-window apps (UIApplicationSceneManifest), we need to explicitly
			// activate the app and switch to its window for element lookup to work.
			// Without this, XCTest only sees the menu bar, not the window content.
			ActivateAppWindow(config);
		}

		// Activates the app and switches to its main window for multi-window scene-based apps
		private void ActivateAppWindow(IConfig config)
		{
			try
			{
				var appId = config.GetProperty<string>("AppId");
				if (string.IsNullOrWhiteSpace(appId))
					return;

				// Activate the application
				_driver?.ExecuteScript("macos: activateApp", new Dictionary<string, object>
				{
					{ "bundleId", appId }
				});

				// Switch to the app's window context
				var windowHandles = _driver?.WindowHandles;
				if (windowHandles != null && windowHandles.Count > 0)
				{
					_driver?.SwitchTo().Window(windowHandles[0]);
				}
			}
			catch (Exception)
			{
				// Ignore activation errors - app may already be active or not support multi-window
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

		// Override to avoid XPath queries which fail on mac2 driver with multi-window apps.
		// XPath causes XQueryError:6 "invalid type" when UIApplicationSceneManifest is configured.

		// On Mac Catalyst, AutomationId maps to accessibilityIdentifier.
		// First search normally, then try searching within the window context for multi-window apps.
		public override IUIElement FindElement(string id)
		{
			// Try accessibility identifier first (AutomationId maps to this on Mac)
			var byAccessibility = AppiumQuery.ByAccessibilityId(id).FindElement(this);
			if (byAccessibility != null)
				return byAccessibility;

			// Try finding within window context for multi-window apps
			var window = FindWindow();
			if (window != null)
			{
				var byWindowSearch = window.FindElements(MobileBy.AccessibilityId(id)).FirstOrDefault();
				if (byWindowSearch != null)
					return new AppiumDriverElement((AppiumElement)byWindowSearch, this);
			}

			// Fall back to standard Id lookup
			return Query.ById(id).FirstOrDefault()!;
		}

		public override IReadOnlyCollection<IUIElement> FindElements(string id)
		{
			// Try accessibility identifier first
			var byAccessibility = AppiumQuery.ByAccessibilityId(id).FindElements(this);
			if (byAccessibility != null && byAccessibility.Count > 0)
				return byAccessibility;

			// Try finding within window context for multi-window apps
			var window = FindWindow();
			if (window != null)
			{
				var byWindowSearch = window.FindElements(MobileBy.AccessibilityId(id));
				if (byWindowSearch != null && byWindowSearch.Count > 0)
					return byWindowSearch.Select(e => new AppiumDriverElement((AppiumElement)e, this)).ToList();
			}

			// Fall back to standard Id lookup
			return Query.ById(id);
		}

		public override IUIElement FindElementByText(string text)
		{
			// Try accessibility identifier first (most reliable for AutomationId)
			var byAccessibility = AppiumQuery.ByAccessibilityId(text).FindElement(this);
			if (byAccessibility != null)
				return byAccessibility;

			// Try name lookup (maps to accessibility label on macOS)
			var byName = AppiumQuery.ByName(text).FindElement(this);
			if (byName != null)
				return byName;

			// Try finding within window context for multi-window apps
			var window = FindWindow();
			if (window != null)
			{
				var byWindowAccessibilityId = window.FindElements(MobileBy.AccessibilityId(text)).FirstOrDefault();
				if (byWindowAccessibilityId != null)
					return new AppiumDriverElement((AppiumElement)byWindowAccessibilityId, this);

				var byWindowName = window.FindElements(MobileBy.Name(text)).FirstOrDefault();
				if (byWindowName != null)
					return new AppiumDriverElement((AppiumElement)byWindowName, this);
			}

			// Return null if not found - avoid XPath fallback
			return null!;
		}

		public override IReadOnlyCollection<IUIElement> FindElementsByText(string text)
		{
			// Try accessibility identifier first
			var byAccessibility = AppiumQuery.ByAccessibilityId(text).FindElements(this);
			if (byAccessibility != null && byAccessibility.Count > 0)
				return byAccessibility;

			// Try name lookup
			var byName = AppiumQuery.ByName(text).FindElements(this);
			if (byName != null && byName.Count > 0)
				return byName;

			// Try finding within window context for multi-window apps
			var window = FindWindow();
			if (window != null)
			{
				var byWindowAccessibilityId = window.FindElements(MobileBy.AccessibilityId(text));
				if (byWindowAccessibilityId != null && byWindowAccessibilityId.Count > 0)
					return byWindowAccessibilityId.Select(e => new AppiumDriverElement((AppiumElement)e, this)).ToList();

				var byWindowName = window.FindElements(MobileBy.Name(text));
				if (byWindowName != null && byWindowName.Count > 0)
					return byWindowName.Select(e => new AppiumDriverElement((AppiumElement)e, this)).ToList();
			}

			return new List<IUIElement>();
		}

		// Helper to find the main window element for window-scoped searches
		private AppiumElement? FindWindow()
		{
			try
			{
				// Find window by class - works better with multi-window apps
				return (AppiumElement?)Driver.FindElements(MobileBy.ClassName("XCUIElementTypeWindow")).FirstOrDefault();
			}
			catch
			{
				return null;
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
