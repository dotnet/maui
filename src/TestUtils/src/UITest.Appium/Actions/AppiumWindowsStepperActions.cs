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
		string? elementId = parameters["elementId"].ToString();

		if (elementId is null)
			return CommandResponse.FailedEmptyResponse;

		var increaseButton = _appiumApp.FindElement(elementId + "Plus");
		increaseButton?.Click();
		return CommandResponse.SuccessEmptyResponse;
	}

	CommandResponse Decrease(IDictionary<string, object> parameters)
	{
		string? elementId = parameters["elementId"].ToString();

		if (elementId is null)
			return CommandResponse.FailedEmptyResponse;

		var decreaseButton = _appiumApp.FindElement(elementId + "Minus");
		decreaseButton?.Click();

		return CommandResponse.SuccessEmptyResponse;
	}
}