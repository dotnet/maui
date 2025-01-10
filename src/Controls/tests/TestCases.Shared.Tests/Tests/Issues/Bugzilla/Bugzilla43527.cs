using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla43527 : _IssuesUITest
{

	public Bugzilla43527(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[UWP] Detail title does not update when wrapped in a NavigationPage";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void TestB43527UpdateTitle()
	{
		App.WaitForElement("Change Title");
		App.WaitForElement("Test Page");
		App.Tap("Change Title");
		App.WaitForElement("New Title");
	}
}