using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumAppleAlertActions : ICommandExecutionGroup
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

		public AppiumAppleAlertActions(AppiumApp appiumApp)
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
			// first try the type used on iOS
			var alerts = _appiumApp.FindElements(AppiumQuery.ByClass("XCUIElementTypeAlert"));

			// then try the type used on macOS
			if (alerts is null || alerts.Count == 0)
				alerts = _appiumApp.FindElements(AppiumQuery.ByClass("XCUIElementTypeSheet"));

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

		static AppiumElement? GetAppiumElement(object element) =>
			element switch
			{
				AppiumElement appiumElement => appiumElement,
				AppiumDriverElement driverElement => driverElement.AppiumElement,
				_ => null
			};
	}
}
