#if WINDOWs || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2414 : _IssuesUITest
{
	public Issue2414(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "NullReferenceException when swiping over Context Actions";

	[Test]
	[Category(UITestCategories.TableView)]
	public void TestShowContextMenuItemsInTheRightOrder()
	{
		App.WaitForElement("Swipe ME");
		App.ContextActions("Swipe ME");
		App.WaitForElement("Text0");
		VerifyScreenshot();
		App.Tap("Text0");
	}
}
#endif