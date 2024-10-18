using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla60524 : _IssuesUITest
{
	public Bugzilla60524(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "NRE when rendering ListView with grouping enabled and HasUnevenRows set to true";

	[Test]
	[Category(UITestCategories.ListView)]
	[FailsOnIOS]
	public void Bugzilla60524Test()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		var group1 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Group 1" + "']"));
		ClassicAssert.NotNull(group1);
	}
}
