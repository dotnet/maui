using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7396 : _IssuesUITest
{
	public Issue7396(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Setting Shell.BackgroundColor overrides all colors of TabBar";

	// TODO: Marked as ManualReview in original test, can we somehow automate this?
	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void BottomTabColorTest()
	//{
	//	//7396 Issue | Shell: Setting Shell.BackgroundColor overrides all colors of TabBar
	//	App.WaitForElement(CreateBottomTabButton);
	//	App.Tap(CreateBottomTabButton);
	//	App.Tap(CreateBottomTabButton);
	//	App.Tap(ChangeShellColorButton);
	//	App.Screenshot("I should see a bottom tabbar icon");
	//	Assert.Inconclusive("Check that bottom tabbar icon is visible");
	//}
}