using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Windows;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumDeviceActions : ICommandExecutionGroup
	{
		const string LockCommand = "lock";
		const string UnlockCommand = "unlock";
		const string IsLockedCommand = "isLocked";
		const string StartRecordingScreenCommand = "startRecordingScreen";
		const string StopRecordingScreenCommand = "stopRecordingScreen";

		readonly AppiumApp _appiumApp;

		readonly List<string> _commands = new()
		{
			LockCommand,
			UnlockCommand,
			IsLockedCommand,
			StartRecordingScreenCommand,
			StopRecordingScreenCommand,
		};

		public AppiumDeviceActions(AppiumApp appiumApp)
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
				IsLockedCommand => IsLocked(parameters),
				StartRecordingScreenCommand => StartRecordingScreen(parameters),
				StopRecordingScreenCommand => StopRecordingScreen(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse Lock(IDictionary<string, object> parameters)
		{
			if (_appiumApp.Driver is AndroidDriver androidDriver)
			{
				androidDriver.Lock();

				return CommandResponse.SuccessEmptyResponse;
			}
			if (_appiumApp.Driver is IOSDriver iOSDriver)
			{
				iOSDriver.Lock();

				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse Unlock(IDictionary<string, object> parameters)
		{
			if (_appiumApp.Driver is AndroidDriver androidDriver)
			{
				string unlockType = (string)parameters["unlockType"];
				string unlockKey = (string)parameters["unlockKey"];

				androidDriver.Unlock(unlockType, unlockKey);

				return CommandResponse.SuccessEmptyResponse;
			}
			if (_appiumApp.Driver is IOSDriver iOSDriver)
			{
				iOSDriver.Unlock();

				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse IsLocked(IDictionary<string, object> parameters)
		{
			if (_appiumApp.Driver is AndroidDriver androidDriver)
			{
				return new CommandResponse(androidDriver.IsLocked(), CommandResponseResult.Success);
			}
			if (_appiumApp.Driver is IOSDriver iOSDriver)
			{
				return new CommandResponse(iOSDriver.IsLocked(), CommandResponseResult.Success);
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
			if (_appiumApp.Driver is IOSDriver iOSDriver)
			{
				iOSDriver.StartRecordingScreen();

				return CommandResponse.SuccessEmptyResponse;
			}
			if (_appiumApp.Driver is WindowsDriver windowsDriver)
			{
				windowsDriver.StartRecordingScreen();

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
			if (_appiumApp.Driver is IOSDriver iOSDriver)
			{
				iOSDriver.StopRecordingScreen();

				return CommandResponse.SuccessEmptyResponse;
			}
			if (_appiumApp.Driver is WindowsDriver windowsDriver)
			{
				windowsDriver.StopRecordingScreen();

				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
		}
	}
}