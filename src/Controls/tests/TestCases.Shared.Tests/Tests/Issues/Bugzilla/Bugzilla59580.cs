#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla59580 : _IssuesUITest
{
	public Bugzilla59580(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Raising Command.CanExecutChanged causes crash on Android";

	[Test]
	[Category(UITestCategories.TableView)]
	[FailsOnIOS]
	public void RaisingCommandCanExecuteChangedCausesCrashOnAndroid()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		App.ActivateContextMenu("Cell");
		var item = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Fire CanExecuteChanged" + "']"));
		item.Click();
	}
}
#endif