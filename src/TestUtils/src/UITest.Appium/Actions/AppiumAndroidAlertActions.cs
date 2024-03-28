using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumAndroidAlertActions : ICommandExecutionGroup
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

		public AppiumAndroidAlertActions(AppiumApp appiumApp)
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
			var alerts = _appiumApp.Query.ById("parentPanel");

			if (alerts is null || alerts.Count == 0)
				return CommandResponse.FailedEmptyResponse;

			return new CommandResponse(alerts, CommandResponseResult.Success);
		}

		CommandResponse GetAlertButtons(IDictionary<string, object> parameters)
		{
			var alert = GetAppiumElement(parameters["element"]);
			if (alert is null)
				return CommandResponse.FailedEmptyResponse;

			var items = AppiumQuery.ByClass("android.widget.ListView")
				.FindElements(alert, _appiumApp)
				.FirstOrDefault()
				?.ByClass("android.widget.TextView");

			var buttons = AppiumQuery.ByClass("android.widget.Button")
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

			var text = AppiumQuery.ByClass("android.widget.TextView").FindElements(alert, _appiumApp);
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
