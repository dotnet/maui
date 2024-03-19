using OpenQA.Selenium.Appium.Android;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumAndroidSpecificActions : ICommandExecutionGroup
	{
		const string LockCommand = "lock";
		const string UnlockCommand = "unlock";
		const string StartRecordingScreenCommand = "startRecordingScreen";
		const string StopRecordingScreenCommand = "stopRecordingScreen";
		const string ToggleAirplaneModeCommand = "toggleAirplaneMode";
		const string ToggleWifiCommand = "toggleWifi";
		const string GetPerformanceDataCommand = "getPerformanceData";

		readonly AppiumApp _appiumApp;

		readonly List<string> _commands = new()
		{
			LockCommand,
			UnlockCommand,
			StartRecordingScreenCommand,
			StopRecordingScreenCommand,
			ToggleAirplaneModeCommand,
			ToggleWifiCommand,
			GetPerformanceDataCommand,
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
				LockCommand => Lock(parameters),
				UnlockCommand => Unlock(parameters),
				StartRecordingScreenCommand => StartRecordingScreen(parameters),
				StopRecordingScreenCommand => StopRecordingScreen(parameters),
				ToggleAirplaneModeCommand => ToggleAirplaneMode(parameters),
				ToggleWifiCommand => ToggleWifi(parameters),
				GetPerformanceDataCommand => GetPerformanceData(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse Lock(IDictionary<string, object> parameters)
		{
			if(_appiumApp.Driver is AndroidDriver androidDriver)
			{
				androidDriver.Lock();

				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse Unlock(IDictionary<string, object> parameters)
		{
			if (_appiumApp.Driver is AndroidDriver androidDriver)
			{
				androidDriver.Unlock();

				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse StartRecordingScreen(IDictionary<string, object> parameters)
		{
			if (_appiumApp.Driver is AndroidDriver androidDriver)
			{
				androidDriver.StartRecordingScreen();

				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse StopRecordingScreen(IDictionary<string, object> parameters)
		{
			if (_appiumApp.Driver is AndroidDriver androidDriver)
			{
				androidDriver.StopRecordingScreen();

				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
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

				if(string.IsNullOrEmpty(performanceDataType))
				{
					performanceDataType = "memoryinfo";
				}

				// Returns the information of the system state which is supported to read as like cpu, memory, network traffic, and battery.
				IList<object> result = androidDriver.GetPerformanceData(_appiumApp.GetAppId(), performanceDataType);

				return new CommandResponse(result, CommandResponseResult.Success);
			}

			return CommandResponse.FailedEmptyResponse;
		}
	}
}