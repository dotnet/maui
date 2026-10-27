using OpenQA.Selenium.Appium.Android;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumAndroidSpecificActions : ICommandExecutionGroup
	{
		const string MoreButton = "OverflowMenuButton";

		const string ToggleAirplaneModeCommand = "toggleAirplaneMode";
		const string ToggleWifiCommand = "toggleWifi";
		const string ToggleDataCommand = "toggleData";
		const string GetPerformanceDataCommand = "getPerformanceData";
		const string ToggleSystemAnimationsCommand = "toggleSystemAnimations";
		const string GetSystemBarsCommand = "getSystemBars";
		const string GetScreenshotTopInsetCommand = "getAndroidScreenshotTopInset";
		const string ToggleSecondaryToolbarItemsCommand = "toggleSecondaryToolbarItems";
		const string CheckIfGestureNavigationIsEnabledCommand = "checkIfGestureNavigationIsEnabled";

		readonly AppiumApp _appiumApp;

		readonly List<string> _commands = new()
		{
			ToggleAirplaneModeCommand,
			ToggleWifiCommand,
			ToggleDataCommand,
			GetPerformanceDataCommand,
			ToggleSystemAnimationsCommand,
			GetSystemBarsCommand,
			GetScreenshotTopInsetCommand,
			ToggleSecondaryToolbarItemsCommand,
			CheckIfGestureNavigationIsEnabledCommand,
		};

		public AppiumAndroidSpecificActions(AppiumApp appiumApp)
		{
			_appiumApp = appiumApp;
		}

		public bool IsCommandSupported(string commandName)
		{
			return _commands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
		}

		public CommandResponse Execute(string commandName, IDictionary<string, object> parameters)
		{
			return commandName switch
			{
				ToggleAirplaneModeCommand => ToggleAirplaneMode(parameters),
				ToggleWifiCommand => ToggleWifi(parameters),
				ToggleDataCommand => ToggleData(parameters),
				GetPerformanceDataCommand => GetPerformanceData(parameters),
				ToggleSystemAnimationsCommand => ToggleSystemAnimations(parameters),
				GetSystemBarsCommand => GetSystemBars(parameters),
				GetScreenshotTopInsetCommand => GetScreenshotTopInset(parameters),
				ToggleSecondaryToolbarItemsCommand => ToggleSecondaryToolbarItems(parameters),
				CheckIfGestureNavigationIsEnabledCommand => CheckIfGestureNavigationIsEnabled(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse ToggleAirplaneMode(IDictionary<string, object> parameters)
		{
			if (_appiumApp.Driver is AndroidDriver androidDriver)
			{
				// Toggle airplane mode on device.
				var currentConnectivity = androidDriver.ExecuteScript("mobile:getConnectivity") as IDictionary<string, object>;

				if (currentConnectivity is not null && currentConnectivity.TryGetValue("airplaneMode", out var currentState))
				{
					bool newState = !(bool)currentState;

					var connectivityParams = new Dictionary<string, object> { { "airplaneMode", newState } };
					androidDriver.ExecuteScript("mobile:setConnectivity", connectivityParams);

					return CommandResponse.SuccessEmptyResponse;
				}
			}

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse ToggleData(IDictionary<string, object> parameters)
		{
			if (_appiumApp.Driver is AndroidDriver androidDriver)
			{
				// Toggle device data.
				var currentConnectivity = androidDriver.ExecuteScript("mobile:getConnectivity") as IDictionary<string, object>;

				if (currentConnectivity is not null && currentConnectivity.TryGetValue("data", out var currentState))
				{
					bool newState = !(bool)currentState;

					var connectivityParams = new Dictionary<string, object> { { "data", newState } };
					androidDriver.ExecuteScript("mobile:setConnectivity", connectivityParams);

					return CommandResponse.SuccessEmptyResponse;
				}
			}

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse ToggleWifi(IDictionary<string, object> parameters)
		{
			if (_appiumApp.Driver is AndroidDriver androidDriver)
			{
				// Switch the state of the WiFi service
				var currentConnectivity = androidDriver.ExecuteScript("mobile:getConnectivity") as IDictionary<string, object>;

				if (currentConnectivity is not null && currentConnectivity.TryGetValue("wifi", out var currentState))
				{
					bool newState = !(bool)currentState;

					var connectivityParams = new Dictionary<string, object> { { "wifi", newState } };
					androidDriver.ExecuteScript("mobile:setConnectivity", connectivityParams);

					return CommandResponse.SuccessEmptyResponse;
				}
			}

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse CheckIfGestureNavigationIsEnabled(IDictionary<string, object> parameters)
		{
			if (_appiumApp.Driver is AndroidDriver androidDriver)
			{
				var result = ShellHelper.ExecuteShellCommandWithOutput("adb shell cmd overlay list");
				var isEnabled = result.Contains("[x] com.android.internal.systemui.navbar.gestural", StringComparison.OrdinalIgnoreCase);
				return new CommandResponse(isEnabled, CommandResponseResult.Success);
			}

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse GetPerformanceData(IDictionary<string, object> parameters)
		{
			if (_appiumApp.Driver is AndroidDriver androidDriver)
			{
				string performanceDataType = (string)parameters["performanceDataType"];

				if (string.IsNullOrEmpty(performanceDataType))
				{
					performanceDataType = "memoryinfo";
				}

				// Returns the information of the system state which is supported to read as like cpu, memory, network traffic, and battery.
				IList<object> result = androidDriver.GetPerformanceData(_appiumApp.GetAppId(), performanceDataType);

				return new CommandResponse(result, CommandResponseResult.Success);
			}

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse ToggleSystemAnimations(IDictionary<string, object> parameters)
		{
			try
			{
				bool enableSystemAnimations = (bool)parameters["enableSystemAnimations"];

				if (enableSystemAnimations)
				{
					ShellHelper.ExecuteShellCommand($"adb shell settings put global window_animation_scale 0");
					ShellHelper.ExecuteShellCommand($"adb shell settings put global transition_animation_scale 0");
					ShellHelper.ExecuteShellCommand($"adb shell settings put global animator_duration_scale 0");

					return CommandResponse.SuccessEmptyResponse;
				}
				else
				{
					ShellHelper.ExecuteShellCommand($"adb shell settings put global window_animation_scale 1");
					ShellHelper.ExecuteShellCommand($"adb shell settings put global transition_animation_scale 1");
					ShellHelper.ExecuteShellCommand($"adb shell settings put global animator_duration_scale 1");

					return CommandResponse.SuccessEmptyResponse;
				}
			}
			catch
			{
				return CommandResponse.FailedEmptyResponse;
			}
		}

		CommandResponse ToggleSecondaryToolbarItems(IDictionary<string, object> parameters)
		{
			try
			{
				_appiumApp.WaitForElement(MoreButton);
				_appiumApp.Tap(MoreButton);

				return CommandResponse.SuccessEmptyResponse;
			}
			catch
			{
				return CommandResponse.FailedEmptyResponse;
			}
		}

		CommandResponse GetScreenshotTopInset(IDictionary<string, object> parameters)
		{
			if (_appiumApp.Driver is not AndroidDriver androidDriver)
				throw new InvalidOperationException("Android screenshot insets require an Android driver.");

			var bars = androidDriver.GetSystemBars();
			if (!bars.TryGetValue("statusBar", out var value) || value is not IDictionary<string, object> statusBar ||
				!statusBar.TryGetValue("visible", out var visibleValue) || visibleValue is not bool visible)
				throw new InvalidOperationException("Android status-bar visibility is missing.");

			var imageWidth = ReadPixel(parameters, "imageWidth");
			var imageHeight = ReadPixel(parameters, "imageHeight");
			var windowBarHeight = ReadPixel(statusBar, "height");
			if (imageWidth <= 0 || imageHeight <= 0 || windowBarHeight >= imageHeight)
				throw new InvalidOperationException("Android status-bar bounds do not fit the screenshot.");
			if (!visible)
				return new CommandResponse(0, CommandResponseResult.Success);

			if (androidDriver.ExecuteScript("mobile: viewportRect") is not IDictionary<string, object> viewport)
				throw new InvalidOperationException("Android native viewport is missing.");
			var top = ReadPixel(viewport, "top");
			var width = ReadPixel(viewport, "width");
			var height = ReadPixel(viewport, "height");
			if (width != imageWidth || height <= 0 || (long)height + top != imageHeight)
				throw new InvalidOperationException("Android native viewport does not match the captured screenshot.");
			if (top == windowBarHeight)
				return new CommandResponse(top, CommandResponseResult.Success);

			if (androidDriver.Capabilities.GetCapability("viewportRect") is not IDictionary<string, object> initialViewport)
				throw new InvalidOperationException("Android session viewport is missing; the cached inset orientation cannot be verified.");
			var initialWidth = ReadPixel(initialViewport, "width");
			var initialHeight = ReadPixel(initialViewport, "height");
			var initialTop = ReadPixel(initialViewport, "top");
			if (initialWidth <= 0 || initialHeight <= 0)
				throw new InvalidOperationException("Android session viewport has invalid bounds.");

			// UiAutomator2 4.2.1 memoizes the native status-bar height for the session.
			if (initialWidth != width ||
				Math.Sign((long)initialWidth - initialHeight - initialTop) != Math.Sign((long)imageWidth - imageHeight))
			{
				Console.WriteLine("Screenshot orientation/size changed since session creation; retaining the current window-reported status-bar crop.");
				return new CommandResponse(windowBarHeight, CommandResponseResult.Success);
			}

			var initialDensity = ReadPixelValue(androidDriver.Capabilities.GetCapability("deviceScreenDensity"), "deviceScreenDensity");
			var currentDensity = androidDriver.GetDisplayDensity();
			if (initialDensity <= 0 || !float.IsFinite(currentDensity) || currentDensity <= 0 || currentDensity > int.MaxValue)
				throw new InvalidOperationException("Android screenshot density is missing or invalid.");
			if (currentDensity != initialDensity)
			{
				Console.WriteLine("Screenshot density changed since session creation; retaining the current window-reported status-bar crop.");
				return new CommandResponse(windowBarHeight, CommandResponseResult.Success);
			}

			return new CommandResponse(top, CommandResponseResult.Success);

			static int ReadPixel(IDictionary<string, object> values, string key) =>
				values.TryGetValue(key, out var pixel) ? ReadPixelValue(pixel, key)
					: throw new InvalidOperationException($"Android screenshot metric '{key}' is missing.");

			static int ReadPixelValue(object? pixel, string key) => pixel switch
			{
				int number when number >= 0 => number,
				long number when number >= 0 && number <= int.MaxValue => (int)number,
				_ => throw new InvalidOperationException($"Android screenshot metric '{key}' must be a nonnegative integer.")
			};
		}

		CommandResponse GetSystemBars(IDictionary<string, object> parameters)
		{
			if (_appiumApp.Driver is AndroidDriver androidDriver)
			{
				IDictionary<string, object> result = androidDriver.GetSystemBars();

				return new CommandResponse(result, CommandResponseResult.Success);
			}

			return CommandResponse.FailedEmptyResponse;
		}
	}
}