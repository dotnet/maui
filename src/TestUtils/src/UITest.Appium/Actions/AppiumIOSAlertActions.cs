using UITest.Core;

namespace UITest.Appium;

public class AppiumIOSAlertActions : AppiumAppleAlertActions
{
	// Selects VISIBLE "Other" elements that are the direct child of
	// a VISIBLE window AND are OVERLAYED on top of the first window.
	const string PossibleAlertXPath =
		"//XCUIElementTypeWindow[@visible='true']/XCUIElementTypeOther[@visible='true' and @index > 0]";

	public AppiumIOSAlertActions(AppiumApp appiumApp)
		: base(appiumApp)
	{
	}

	protected override IReadOnlyCollection<IUIElement> OnGetAlerts(AppiumApp appiumApp, IDictionary<string, object> parameters)
	{
		// First try the type used on iOS.
		var alerts = appiumApp.FindElements(AppiumQuery.ByClass("XCUIElementTypeAlert"));

		// It appears iOS sometimes uses the XCUIElementTypeOther class for action sheets
		// so we need a way to do a more fuzzy check.
		if (alerts is null || alerts.Count == 0)
			alerts = appiumApp.FindElements(AppiumQuery.ByXPath(PossibleAlertXPath));

		return alerts;
	}
}
