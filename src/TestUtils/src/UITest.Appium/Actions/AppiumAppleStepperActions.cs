using OpenQA.Selenium.Appium;
using UITest.Core;

namespace UITest.Appium;

class AppiumAppleStepperActions : ICommandExecutionGroup
{
	const string IncreaseCommand = "increaseStepper";
	const string DecreaseCommand = "decreaseStepper";

	readonly List<string> _commands = new()
	{
		IncreaseCommand,
		DecreaseCommand
	};

	readonly AppiumApp _appiumApp;

	public AppiumAppleStepperActions(AppiumApp appiumApp)
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
			IncreaseCommand => Increase(parameters),
			DecreaseCommand => Decrease(parameters),
			_ => CommandResponse.FailedEmptyResponse,
		};
	}

	CommandResponse Increase(IDictionary<string, object> parameters)
	{
		string? elementId = parameters["elementId"].ToString();

		if (elementId is null)
			return CommandResponse.FailedEmptyResponse;

		var element = _appiumApp.FindElement(elementId);
		var stepper = GetAppiumElement(element);

		if (stepper is null)
			return CommandResponse.FailedEmptyResponse;

		var buttons = AppiumQuery.ByClass("XCUIElementTypeButton").FindElements(stepper, _appiumApp);

		if (buttons is not null && buttons.Count > 1)
		{
			// On Mac Catalyst, Appium's identifier attribute for the stepper's private UIButton
			// subviews is populated lazily and its readiness is timing-sensitive, which made
			// identifier-based matching flaky in CI. Sort by X position instead: the increase
			// (+) button is always the rightmost button.
			var increaseButton = _appiumApp is AppiumCatalystApp
				? OrderButtonsByPosition(buttons).LastOrDefault()
				: buttons.LastOrDefault();

			increaseButton?.Tap();
		}

		return CommandResponse.SuccessEmptyResponse;
	}

	CommandResponse Decrease(IDictionary<string, object> parameters)
	{
		string? elementId = parameters["elementId"].ToString();

		if (elementId is null)
			return CommandResponse.FailedEmptyResponse;

		var element = _appiumApp.FindElement(elementId);
		var stepper = GetAppiumElement(element);

		if (stepper is null)
			return CommandResponse.FailedEmptyResponse;

		var buttons = AppiumQuery.ByClass("XCUIElementTypeButton").FindElements(stepper, _appiumApp);

		if (buttons is not null && buttons.Count > 1)
		{
			// On Mac Catalyst, Appium's identifier attribute for the stepper's private UIButton
			// subviews is populated lazily and its readiness is timing-sensitive, which made
			// identifier-based matching flaky in CI. Sort by X position instead: the decrease
			// (-) button is always the leftmost button.
			var decreaseButton = _appiumApp is AppiumCatalystApp
				? OrderButtonsByPosition(buttons).FirstOrDefault()
				: buttons.FirstOrDefault();

			decreaseButton?.Tap();
		}

		return CommandResponse.SuccessEmptyResponse;
	}

	// Sorts stepper buttons left-to-right by their X position. Falls back to the
	// original order if reading element geometry fails for any reason.
	static List<IUIElement> OrderButtonsByPosition(IReadOnlyCollection<IUIElement> buttons)
	{
		try
		{
			return buttons.OrderBy(b => GetAppiumElement(b)?.Rect.X ?? 0).ToList();
		}
		catch
		{
			return buttons.ToList();
		}
	}

	static AppiumElement? GetAppiumElement(object element) =>
		element switch
		{
			AppiumElement appiumElement => appiumElement,
			AppiumDriverElement driverElement => driverElement.AppiumElement,
			_ => null
		};
}