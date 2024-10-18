using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla57717 : _IssuesUITest
{
	const string ButtonText = "I am a button";

	public Bugzilla57717(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Setting background color on Button in Android FormsApplicationActivity causes NRE";

	[Test]
	[Category(UITestCategories.Button)]
	[FailsOnIOS]
	public void ButtonBackgroundColorAutomatedTest()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		// With the original bug in place, we'll crash before we get this far
		var button = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + ButtonText + "']"));
		ClassicAssert.NotNull(button);
	}
}
