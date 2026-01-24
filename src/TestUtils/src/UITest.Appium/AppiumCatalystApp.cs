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
		//
		// CRITICAL: For multi-window apps (UIApplicationSceneManifest), we MUST:
		// 1. Ensure the correct window is in focus before searching
		// 2. Search within the window element context, not globally on driver
		// 3. Avoid XPath queries which fail with "invalid type" errors
		//
		// On Mac Catalyst, AutomationId maps to accessibilityIdentifier.

		public override IUIElement FindElement(string id)
		{
			// Ensure we're in the correct window context for multi-window apps
			EnsureWindowContext();

			// For multi-window apps, search within window element first (most reliable)
			var window = FindWindow();
			if (window != null)
			{
				var byWindowSearch = window.FindElements(MobileBy.AccessibilityId(id)).FirstOrDefault();
				if (byWindowSearch != null)
					return new AppiumDriverElement((AppiumElement)byWindowSearch, this);

				// Also try Id within window (some elements use Id instead of AccessibilityId)
				var byWindowId = window.FindElements(MobileBy.Id(id)).FirstOrDefault();
				if (byWindowId != null)
					return new AppiumDriverElement((AppiumElement)byWindowId, this);
			}

			// Fall back to driver-level search (works for single-window apps)
			var byAccessibility = AppiumQuery.ByAccessibilityId(id).FindElement(this);
			if (byAccessibility != null)
				return byAccessibility;

			// Last resort: standard Id lookup
			return Query.ById(id).FirstOrDefault()!;
		}

		public override IReadOnlyCollection<IUIElement> FindElements(string id)
		{
			// Ensure we're in the correct window context for multi-window apps
			EnsureWindowContext();

			// For multi-window apps, search within window element first
			var window = FindWindow();
			if (window != null)
			{
				var byWindowSearch = window.FindElements(MobileBy.AccessibilityId(id));
				if (byWindowSearch != null && byWindowSearch.Count > 0)
					return byWindowSearch.Select(e => new AppiumDriverElement((AppiumElement)e, this)).ToList();

				var byWindowId = window.FindElements(MobileBy.Id(id));
				if (byWindowId != null && byWindowId.Count > 0)
					return byWindowId.Select(e => new AppiumDriverElement((AppiumElement)e, this)).ToList();
			}

			// Fall back to driver-level search
			var byAccessibility = AppiumQuery.ByAccessibilityId(id).FindElements(this);
			if (byAccessibility != null && byAccessibility.Count > 0)
				return byAccessibility;

			return Query.ById(id);
		}

		public override IUIElement FindElementByText(string text)
		{
			// Ensure we're in the correct window context for multi-window apps
			EnsureWindowContext();

			// For multi-window apps, search within window element first
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

			// Fall back to driver-level search
			var byAccessibility = AppiumQuery.ByAccessibilityId(text).FindElement(this);
			if (byAccessibility != null)
				return byAccessibility;

			var byName = AppiumQuery.ByName(text).FindElement(this);
			if (byName != null)
				return byName;

			// Return null if not found - avoid XPath fallback
			return null!;
		}

		public override IReadOnlyCollection<IUIElement> FindElementsByText(string text)
		{
			// Ensure we're in the correct window context for multi-window apps
			EnsureWindowContext();

			// For multi-window apps, search within window element first
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

			// Fall back to driver-level search
			var byAccessibility = AppiumQuery.ByAccessibilityId(text).FindElements(this);
			if (byAccessibility != null && byAccessibility.Count > 0)
				return byAccessibility;

			var byName = AppiumQuery.ByName(text).FindElements(this);
			if (byName != null && byName.Count > 0)
				return byName;

			return new List<IUIElement>();
		}

		// Override query-based FindElement to ensure window context for multi-window apps
		public override IUIElement FindElement(IQuery query)
		{
			EnsureWindowContext();
			return base.FindElement(query);
		}

		// Override query-based FindElements to ensure window context for multi-window apps
		public override IReadOnlyCollection<IUIElement> FindElements(IQuery query)
		{
			EnsureWindowContext();
			return base.FindElements(query);
		}

		// Ensures the driver is focused on the app's window context.
		// This is critical for multi-window apps where the driver may lose window focus.
		private void EnsureWindowContext()
		{
			try
			{
				var windowHandles = _driver?.WindowHandles;
				if (windowHandles != null && windowHandles.Count > 0)
				{
					// Switch to the first available window (main app window)
					_driver?.SwitchTo().Window(windowHandles[0]);
				}
			}
			catch
			{
				// Ignore errors - window may already be in correct context
			}
		}

		// Helper to find the main window element for window-scoped searches
		private AppiumElement? FindWindow()
		{
			try
			{
				// Find window by class - this is the most reliable way for multi-window apps
				// The window element contains all UI content and can be searched within
				var windows = Driver.FindElements(MobileBy.ClassName("XCUIElementTypeWindow"));
				
				// Return the first non-empty window (skip menu bar windows)
				foreach (var window in windows)
				{
					// Check if this window has actual content (not just menu bar)
					var children = window.FindElements(MobileBy.ClassName("XCUIElementTypeOther"));
					if (children.Count > 0)
						return (AppiumElement)window;
				}
				
				// Fall back to first window if no content-rich window found
				return (AppiumElement?)windows.FirstOrDefault();
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
