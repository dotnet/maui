using UITest.Core;

namespace UITest.Appium;

public class AppiumCatalystAlertActions : AppiumAppleAlertActions
{
	// Selects the inner "popover contents" of a popover window.
	const string PossibleActionSheetXPath =
		"/XCUIElementTypeApplication/XCUIElementTypeWindow/XCUIElementTypePopover";

	const string DismissAlertCommand = "dismissAlert";

	readonly List<string> _commands = new()
	{
		DismissAlertCommand,
	};

	public AppiumCatalystAlertActions(AppiumApp appiumApp)
		: base(appiumApp)
	{
	}

	public override bool IsCommandSupported(string commandName) =>
		_commands.Contains(commandName, StringComparer.OrdinalIgnoreCase) || base.IsCommandSupported(commandName);

	public override CommandResponse Execute(string commandName, IDictionary<string, object> parameters) =>
		commandName switch
		{
			DismissAlertCommand => DismissAlert(parameters),
			_ => base.Execute(commandName, parameters),
		};

	CommandResponse DismissAlert(IDictionary<string, object> parameters)
	{
		var alert = GetAppiumElement(parameters["element"]);
		if (alert is null)
			return CommandResponse.FailedEmptyResponse;

		// XCUIElementTypePopover == 18
		if (!"18".Equals(alert.GetAttribute("elementType"), StringComparison.OrdinalIgnoreCase))
			return CommandResponse.FailedEmptyResponse;

		var dismissRegions = AppiumQuery.ById("PopoverDismissRegion").FindElements(_appiumApp).ToList();
		for (var i = dismissRegions.Count - 1; i >= 0; i--)
		{
			var region = GetAppiumElement(dismissRegions[i])!;
			if ("true".Equals(region.GetAttribute("enabled"), StringComparison.OrdinalIgnoreCase))
			{
				region.Click();
				return CommandResponse.SuccessEmptyResponse;
			}
		}

		return CommandResponse.FailedEmptyResponse;
	}

	protected override IReadOnlyCollection<IUIElement> OnGetAlerts(AppiumApp appiumApp, IDictionary<string, object> parameters)
	{
		// Catalyst uses action sheets for alerts and macOS 14
		var alerts = appiumApp.FindElements(AppiumQuery.ByClass("XCUIElementTypeSheet"));

		// But it also uses popovers for action sheets on macOS 13
		if (alerts is null || alerts.Count == 0)
			alerts = appiumApp.FindElements(AppiumQuery.ByXPath(PossibleActionSheetXPath));

		return alerts;
	}
}
