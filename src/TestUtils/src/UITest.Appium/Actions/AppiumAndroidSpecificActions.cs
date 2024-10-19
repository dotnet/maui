using System.Diagnostics;
using OpenQA.Selenium.Appium.Android;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumAndroidSpecificActions : ICommandExecutionGroup
	{
		const string ToggleAirplaneModeCommand = "toggleAirplaneMode";
		const string ToggleWifiCommand = "toggleWifi";
		const string GetPerformanceDataCommand = "getPerformanceData";
		const string SetPropertiesCommand = "setProperties";

		readonly AppiumApp _appiumApp;

		readonly List<string> _commands =
		[
			ToggleAirplaneModeCommand,
			ToggleWifiCommand,
			GetPerformanceDataCommand,
			SetPropertiesCommand
		];

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
				SetPropertiesCommand => SetProperties(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		static CommandResponse SetProperties(IDictionary<string, object> properties)
		{
			if (properties.Count <= 0)
			{
				return CommandResponse.FailedEmptyResponse;
			}

			try
			{
				foreach (var property in properties)
				{
					ExecuteAdbCommand($"adb shell settings put global {property.Key} {property.Value.ToString()}");
				}

			}
			catch (Exception ex)
			{
				return new CommandResponse(ex.Message, CommandResponseResult.Failed);
			}

			return CommandResponse.SuccessEmptyResponse;
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

		// TODO: Make this part of a base class. Too much repetition across tests.
		static void ExecuteAdbCommand(string command, CancellationToken cancellationToken = default)
		{
			var shell = GetShell();
			var shellArgument = GetShellArgument(shell, command);
			var processInfo = new ProcessStartInfo(shell, shellArgument)
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			using var process = new Process { StartInfo = processInfo };
			process.Start();

			var output = process.StandardOutput.ReadToEnd();
			var error = process.StandardError.ReadToEnd();

			process.WaitForExit();

			if (process.ExitCode != 0)
			{
				throw new Exception($"Command failed with exit code {process.ExitCode}: {error}");
			}

			Console.WriteLine(output);
		}

		static string GetShell() => Environment.OSVersion.Platform == PlatformID.Win32NT ? "cmd.exe" : "/bin/bash";
		static string GetShellArgument(string shell, string command) => shell == "cmd.exe" ? $"/C {command}" : $"-c \"{command}\"" ;
	}
}