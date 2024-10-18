using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3509 : _IssuesUITest
{
	const string _popPage = "Pop Page";

	public Issue3509(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] NavigationPage.Popped called twice when Navigation.PopAsync is called";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void PoppedOnlyFiresOnce()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		var popPage = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + _popPage + "']"));
		popPage.Click();

		var result = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "1" + "']"));
		ClassicAssert.NotNull(result);

		App.Back();
	}
}