using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue15649(TestDevice testDevice) : _IssuesUITest(testDevice)
{
	public override string Issue => "Updating a ControlTemplate at runtime for a Content Page is not working.";

	[Test]
	[Category(UITestCategories.Page)]
	public void DynamicallyUpdatingContentPage()
	{
		App.WaitForElement("Page1Label");
		App.Tap("Button");
		App.WaitForElement("Page2Label");
	}
}
