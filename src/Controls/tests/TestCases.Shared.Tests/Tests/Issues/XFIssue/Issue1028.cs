using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1028 : _IssuesUITest
{
	public Issue1028(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ViewCell in TableView raises exception - root page is ContentPage, Content is TableView";

	[Test]
	[Category(UITestCategories.TableView)]
	public void ViewCellInTableViewDoesNotCrash()
	{
		// If we can see this element, then we didn't crash.
		App.WaitForElement("Custom Slider View:");
	}
}