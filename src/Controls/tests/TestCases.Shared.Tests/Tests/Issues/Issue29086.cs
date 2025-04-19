using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29086 : _IssuesUITest
{

	public Issue29086(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "SwipeView Closes when Content Changes even with SwipeBehaviorOnInvoked='RemainOpen'";

	[Test]
	[Category(UITestCategories.SwipeView)]
	public void SwipeViewShouldNotClose()
	{
		App.WaitForElement("SwipeItem");
		App.SwipeLeftToRight("SwipeItem");
		App.Click("AddButton");
		App.Click("AddButton");
		VerifyScreenshot();
	}
}