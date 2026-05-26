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