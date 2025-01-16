using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue416 : _IssuesUITest
{
	public Issue416(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "NavigationPage in PushModal does not show NavigationBar";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void Issue416TestsNavBarPresent()
	{
		App.WaitForElement("Test Page");
		App.WaitForElement("I should have a nav bar");
	}
}