using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Mac;
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
			IncreaseCommand => ChangeValue(parameters, increase: true),
			DecreaseCommand => ChangeValue(parameters, increase: false),
			_ => CommandResponse.FailedEmptyResponse,
		};
	}

	CommandResponse ChangeValue(IDictionary<string, object> parameters, bool increase)
	{
		if (!parameters.TryGetValue("elementId", out var value) || value is not string elementId || string.IsNullOrEmpty(elementId))
			throw new ArgumentException("A Stepper automation identifier is required.", nameof(parameters));

		var element = _appiumApp.FindElement(elementId);
		var stepper = GetAppiumElement(element);

		if (stepper is null)
			throw new InvalidOperationException($"Stepper '{elementId}' is not an Appium element.");

		var buttons = AppiumQuery.ByClass("XCUIElementTypeButton").FindElements(stepper, _appiumApp);

		if (buttons is not null && buttons.Count > 1)
		{
			var isMac = _appiumApp.Driver is MacDriver;
			var identifiedButtons = buttons.Select(button =>
			{
				var nativeButton = GetAppiumElement(button)
					?? throw new InvalidOperationException($"Stepper '{elementId}' has a non-Appium button.");
				return (Button: button, Identifier: nativeButton.GetAttribute(isMac ? "identifier" : "name"));
			}).ToArray();
			var incrementId = elementId + "-Increment";
			var decrementId = elementId + "-Decrement";
			var increments = identifiedButtons.Where(button => button.Identifier == incrementId).ToArray();
			var decrements = identifiedButtons.Where(button => button.Identifier == decrementId).ToArray();
			var semanticCount = identifiedButtons.Count(button =>
				button.Identifier == incrementId || button.Identifier == decrementId
				|| isMac && (button.Identifier?.EndsWith("-Increment", StringComparison.Ordinal) == true
					|| button.Identifier?.EndsWith("-Decrement", StringComparison.Ordinal) == true));

			// Only unidentified legacy pairs retain the positional contract; identified buttons can mirror in RTL.
			var button = increase ? buttons.Last() : buttons.First();
			if (semanticCount > 0)
			{
				if (semanticCount != 2 || increments.Length != 1 || decrements.Length != 1)
					throw new InvalidOperationException($"Stepper '{elementId}' does not expose a unique increment/decrement button pair.");

				button = increase ? increments[0].Button : decrements[0].Button;
			}
			button.Tap();
			return CommandResponse.SuccessEmptyResponse;
		}

		if (!parameters.TryGetValue("isRightToLeft", out var direction) || direction is not bool isRightToLeft)
			throw new InvalidOperationException($"Stepper '{elementId}' has no accessible button pair. Supply its effective isRightToLeft direction for a coordinate tap.");

		var bounds = stepper.Rect;
		if (bounds.Width <= 0 || bounds.Height <= 0)
			throw new InvalidOperationException($"Stepper '{elementId}' has empty bounds and cannot be tapped.");

		var rightHalf = increase != isRightToLeft;
		var x = bounds.X + bounds.Width * (rightHalf ? 0.75f : 0.25f);
		var y = bounds.Y + bounds.Height / 2f;
		_appiumApp.TapCoordinates(x, y);
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