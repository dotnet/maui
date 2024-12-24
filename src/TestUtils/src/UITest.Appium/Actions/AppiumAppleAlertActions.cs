using OpenQA.Selenium.Appium;
using UITest.Core;

namespace UITest.Appium;

public abstract class AppiumAppleAlertActions : ICommandExecutionGroup
{
	const string GetAlertsCommand = "getAlerts";
	const string GetAlertButtonsCommand = "getAlertButtons";
	const string GetAlertTextCommand = "getAlertText";

	readonly List<string> _commands = new()
	{
		GetAlertsCommand,
		GetAlertButtonsCommand,
		GetAlertTextCommand,
	};

	protected readonly AppiumApp _appiumApp;

	public AppiumAppleAlertActions(AppiumApp appiumApp)
	{
		_appiumApp = appiumApp;
	}

	public virtual bool IsCommandSupported(string commandName) =>
		_commands.Contains(commandName, StringComparer.OrdinalIgnoreCase);

	public virtual CommandResponse Execute(string commandName, IDictionary<string, object> parameters) =>
		commandName switch
		{
			GetAlertsCommand => GetAlerts(parameters),
			GetAlertButtonsCommand => GetAlertButtons(parameters),
			GetAlertTextCommand => GetAlertText(parameters),
			_ => CommandResponse.FailedEmptyResponse,
		};

	protected abstract IReadOnlyCollection<IUIElement> OnGetAlerts(AppiumApp appiumApp, IDictionary<string, object> parameters);

	CommandResponse GetAlerts(IDictionary<string, object> parameters)
	{
		var alerts = OnGetAlerts(_appiumApp, parameters);

		if (alerts is null || alerts.Count == 0)
			return CommandResponse.FailedEmptyResponse;

		return new CommandResponse(alerts, CommandResponseResult.Success);
	}

	CommandResponse GetAlertButtons(IDictionary<string, object> parameters)
	{
		var alert = GetAppiumElement(parameters["element"]);
		if (alert is null)
			return CommandResponse.FailedEmptyResponse;

		var buttons = AppiumQuery.ByClass("XCUIElementTypeButton").FindElements(alert, _appiumApp);

		return new CommandResponse(buttons, CommandResponseResult.Success);
	}

	CommandResponse GetAlertText(IDictionary<string, object> parameters)
	{
		var alert = GetAppiumElement(parameters["element"]);
		if (alert is null)
			return CommandResponse.FailedEmptyResponse;

		var text = AppiumQuery.ByClass("XCUIElementTypeStaticText").FindElements(alert, _appiumApp);
		var strings = text.Select(t => t.GetText()).ToList();

		return new CommandResponse(strings, CommandResponseResult.Success);
	}

	protected static AppiumElement? GetAppiumElement(object element) =>
		element switch
		{
			AppiumElement appiumElement => appiumElement,
			AppiumDriverElement driverElement => driverElement.AppiumElement,
			_ => null
		};
}
