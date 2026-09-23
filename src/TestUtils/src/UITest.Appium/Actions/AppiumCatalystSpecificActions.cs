using System.Diagnostics;
using System.Drawing;
using UITest.Core;

namespace UITest.Appium;

public class AppiumCatalystSpecificActions : ICommandExecutionGroup
{
	const string EnterFullScreenCommand = "enterFullScreen";
	const string ExitFullScreenCommand = "exitFullScreen";
	const string ToggleSystemAnimationsCommand = "toggleSystemAnimations";

	readonly AppiumApp _appiumApp;
	Size? _windowedSize;

	readonly List<string> _commands = new()
	{
		EnterFullScreenCommand,
		ExitFullScreenCommand,
		ToggleSystemAnimationsCommand,
	};

	public AppiumCatalystSpecificActions(AppiumApp appiumApp)
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
			EnterFullScreenCommand => EnterFullScreen(parameters),
			ExitFullScreenCommand => ExitFullScreen(parameters),
			ToggleSystemAnimationsCommand => ToggleSystemAnimations(parameters),
			_ => CommandResponse.FailedEmptyResponse,
		};
	}

	CommandResponse EnterFullScreen(IDictionary<string, object> parameters)
	{
		var windowedSize = GetWindowSize();
		_windowedSize = windowedSize;
		_appiumApp.Driver.Manage().Window.FullScreen();
		WaitForWindowSize(size => size != windowedSize, "enter full screen");
		return CommandResponse.SuccessEmptyResponse;
	}

	CommandResponse ExitFullScreen(IDictionary<string, object> parameters)
	{
		var windowedSize = _windowedSize
			?? throw new InvalidOperationException("EnterFullScreen must capture the window size before exiting full screen.");

		string[] keys = ["XCUIKeyboardKeyEscape"];
		_appiumApp.Driver.ExecuteScript("macos: keys", new Dictionary<string, object>
		{
			{ "keys", keys },
		});

		// macOS persists full-screen state on termination, so wait before the fixture closes the app.
		WaitForWindowSize(size => size == windowedSize, "restore its windowed size");
		_windowedSize = null;
		return CommandResponse.SuccessEmptyResponse;
	}

	Size GetWindowSize() =>
		_appiumApp.WaitForElement(AppiumQuery.ByXPath("//XCUIElementTypeWindow")).GetRect().Size;

	void WaitForWindowSize(Func<Size, bool> condition, string transition)
	{
		var stopwatch = Stopwatch.StartNew();
		Size size;
		do
		{
			size = GetWindowSize();
			if (condition(size))
				return;

			Thread.Sleep(100);
		}
		while (stopwatch.Elapsed < TimeSpan.FromSeconds(15));

		throw new TimeoutException($"Mac app did not {transition}; its window is {size.Width}x{size.Height}.");
	}

	CommandResponse ToggleSystemAnimations(IDictionary<string, object> parameters)
	{
		try
		{
			bool enableSystemAnimations = (bool)parameters["enableSystemAnimations"];

			if (enableSystemAnimations)
			{
				// Disable Window Animations.
				ShellHelper.ExecuteShellCommand($"defaults write NSGlobalDomain NSAutomaticWindowAnimationsEnabled -bool false");

				// Increase the speed of OSX dialogs boxes.
				ShellHelper.ExecuteShellCommand($"defaults write NSGlobalDomain NSWindowResizeTime .1");

				return CommandResponse.SuccessEmptyResponse;
			}
			else
			{
				ShellHelper.ExecuteShellCommand($"defaults write NSGlobalDomain NSAutomaticWindowAnimationsEnabled -bool true");
				ShellHelper.ExecuteShellCommand($"defaults write NSGlobalDomain NSWindowResizeTime .5");

				return CommandResponse.SuccessEmptyResponse;
			}
		}
		catch
		{
			return CommandResponse.FailedEmptyResponse;
		}
	}
}