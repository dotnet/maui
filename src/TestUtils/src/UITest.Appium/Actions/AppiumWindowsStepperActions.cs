using OpenQA.Selenium.Appium;
using UITest.Core;

namespace UITest.Appium;

public class AppiumWindowsStepperActions : ICommandExecutionGroup
{
	const string IncreaseCommand = "increaseStepper";
	const string DecreaseCommand = "decreaseStepper";

	readonly List<string> _commands = new()
	{
		IncreaseCommand,
		DecreaseCommand
	};
	readonly AppiumApp _appiumApp;

	public AppiumWindowsStepperActions(AppiumApp appiumApp)
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
		var stepper = GetAppiumElement(parameters["element"]);

		if (stepper is null)
			return CommandResponse.FailedEmptyResponse;

		var buttons = AppiumQuery.ByXPath("//Button").FindElements(stepper, _appiumApp);

		var increaseButton = AppiumQuery.ByXPath("//*[@text='\" + \"+\" + \"']\"")
			.FindElements(stepper, _appiumApp)
			.FirstOrDefault();

		increaseButton?.Tap();

		return CommandResponse.SuccessEmptyResponse;
	}

	CommandResponse Decrease(IDictionary<string, object> parameters)
	{
		var stepper = GetAppiumElement(parameters["element"]);

		if (stepper is null)
			return CommandResponse.FailedEmptyResponse;

		var decreaseButton = AppiumQuery.ByXPath("//*[@text='\" + \"-\" + \"']\"")
			.FindElements(stepper, _appiumApp)
			.FirstOrDefault();

		decreaseButton?.Tap();

		return CommandResponse.SuccessEmptyResponse;
	}

	static AppiumElement? GetAppiumElement(object element) =>
		element switch
		{
			AppiumElement appiumElement => appiumElement,
			AppiumDriverElement driverElement => driverElement.AppiumElement,
			_ => null
		};
}