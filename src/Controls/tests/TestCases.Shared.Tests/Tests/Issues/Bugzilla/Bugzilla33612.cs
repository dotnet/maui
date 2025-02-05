using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla33612 : _IssuesUITest
{
	public Bugzilla33612(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "(A) Removing a page from the navigation stack causes an 'Object reference' exception in Android only";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void Issue33612RemovePagesWithoutRenderers()
	{
		App.WaitForElement("Go To Page 2");
		App.Tap("Go To Page 2");

		App.WaitForElement("This is Page 2");
		App.Tap("Go To Page 3");

		App.WaitForElement("This is Page 3");
		App.WaitForElement("Return To Page 2");
		App.Tap("Return To Page 2");

		App.WaitForElement("If you are seeing this, nothing crashed.");
	}
}
