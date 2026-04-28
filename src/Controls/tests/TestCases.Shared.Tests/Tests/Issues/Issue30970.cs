using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30970 : _IssuesUITest
{
	public Issue30970(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Alert popup may be displayed on wrong window when modal page navigation is in progress";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void PopupShouldBeDisplayed()
	{
		App.WaitForElement("OpenModalButton");
		App.Click("OpenModalButton");
		App.WaitForElement("CloseModalButton");
		App.Click("CloseModalButton");
		App.WaitForElement("Can you see this alert?");
	}
}