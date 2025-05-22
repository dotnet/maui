using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using UITest.Core;

namespace UITest.Appium;

public class AppiumWindowsAlertActions : ICommandExecutionGroup
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
	readonly AppiumApp _appiumApp;

	public AppiumWindowsAlertActions(AppiumApp appiumApp)
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
			GetAlertsCommand => GetAlerts(parameters),
			GetAlertButtonsCommand => GetAlertButtons(parameters),
			GetAlertTextCommand => GetAlertText(parameters),
			_ => CommandResponse.FailedEmptyResponse,
		};
	}

	CommandResponse GetAlerts(IDictionary<string, object> parameters)
	{
		var result = _appiumApp.Driver.FindElements(By.XPath("//Window[@ClassName=\"Popup\"][@IsModal=\"True\"]"));

		if (result is null || result.Count == 0)
			return CommandResponse.FailedEmptyResponse;

		var alerts = result.Select(e => new AppiumDriverElement(e, _appiumApp)).ToList();

		return new CommandResponse(alerts, CommandResponseResult.Success);
	}

	CommandResponse GetAlertButtons(IDictionary<string, object> parameters)
	{
		var alert = GetAppiumElement(parameters["element"]);
		if (alert is null)
			return CommandResponse.FailedEmptyResponse;

		var items = AppiumQuery.ByClass("ListView")
			.FindElements(alert, _appiumApp)
			.FirstOrDefault()
			?.ByClass("TextBlock");

		var buttons = AppiumQuery.ByClass("Button")
			.FindElements(alert, _appiumApp);

		var all = new List<IUIElement>();
		if (items is not null)
			all.AddRange(items);
		all.AddRange(buttons);

		return new CommandResponse(all, CommandResponseResult.Success);
	}

	CommandResponse GetAlertText(IDictionary<string, object> parameters)
	{
		var alert = GetAppiumElement(parameters["element"]);

		if (alert is null)
			return CommandResponse.FailedEmptyResponse;

		var text = AppiumQuery.ByClass("TextBlock")
			.FindElements(alert, _appiumApp);

		var strings = text.Select(t => t.GetText()).ToList();

		return new CommandResponse(strings, CommandResponseResult.Success);
	}

	static AppiumElement? GetAppiumElement(object element) =>
		element switch
		{
			AppiumElement appiumElement => appiumElement,
			AppiumDriverElement driverElement => driverElement.AppiumElement,
			_ => null
		};
}
