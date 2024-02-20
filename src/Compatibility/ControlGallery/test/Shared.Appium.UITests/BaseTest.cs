using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace UITests;

public abstract class BaseTest
{
	protected AppiumDriver App => AppiumSetup.App;

	// This could also be an extension method to AppiumDriver if you prefer
	protected AppiumElement FindUIElement(string id)
	{
		if (App is WindowsDriver)
		{
			return App.FindElement(MobileBy.AccessibilityId(id));
		}

		return App.FindElement(MobileBy.Id(id));
	}
}