using OpenQA.Selenium.Appium.Android;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumAndroidSpecificActions : ICommandExecutionGroup
	{
		const string ToggleAirplaneModeCommand = "toggleAirplaneMode";
		const string ToggleWifiCommand = "toggleWifi";
		const string GetPerformanceDataCommand = "getPerformanceData";
		const string ToggleSystemAnimationsCommand = "toggleSystemAnimations";

		readonly AppiumApp _appiumApp;

		readonly List<string> _commands = new()
		{
			ToggleAirplaneModeCommand,
			ToggleWifiCommand,
			GetPerformanceDataCommand,
			ToggleSystemAnimationsCommand,
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
				GetPerformanceDataCommand => GetPerformanceData(parameters),
				ToggleSystemAnimationsCommand => ToggleSystemAnimations(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse ToggleAirplaneMode(IDictionary<string, object> parameters)
		{
			if (_appiumApp.Driver is AndroidDriver androidDriver)
			{
				// Toggle airplane mode on device.
				androidDriver.ToggleAirplaneMode();

				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse ToggleWifi(IDictionary<string, object> parameters)
		{
			if (_appiumApp.Driver is AndroidDriver androidDriver)
			{
				// Switch the state of the wifi service
				androidDriver.ToggleWifi();

				return CommandResponse.SuccessEmptyResponse;
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
					ShellHelper.ExecuteAdbCommand($"adb shell settings put global window_animation_scale 0");
					ShellHelper.ExecuteAdbCommand($"adb shell settings put global transition_animation_scale 0");
					ShellHelper.ExecuteAdbCommand($"adb shell settings put global animator_duration_scale 0");

					return CommandResponse.SuccessEmptyResponse;
				}
				else
				{
					ShellHelper.ExecuteAdbCommand($"adb shell settings put global window_animation_scale 1");
					ShellHelper.ExecuteAdbCommand($"adb shell settings put global transition_animation_scale 1");
					ShellHelper.ExecuteAdbCommand($"adb shell settings put global animator_duration_scale 1");

					return CommandResponse.SuccessEmptyResponse;
				}
			}
			catch
			{
				return CommandResponse.FailedEmptyResponse;
			}
		}
	}
}