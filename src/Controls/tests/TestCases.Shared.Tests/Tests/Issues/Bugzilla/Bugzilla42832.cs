#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla42832 : _IssuesUITest
{
	public Bugzilla42832(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Scrolling a ListView with active ContextAction Items causes NRE";

	// TODO From Xamarin.UITest migration: test failed
	// [Test]
	// [Category(UITestCategories.ListView)]
	// public void ContextActionsScrollNRE()
	// {
	// 	App.TouchAndHold("Item #0");
	// 	App.WaitForElement("Test Item");

	// 	int counter = 0;
	// 	while(counter < 5)
	// 	{
	// 		App.ScrollDown("Item #15", ScrollStrategy.Gesture);
	// 		App.ScrollUp("Item #0", ScrollStrategy.Gesture);
	// 		counter++;
	// 	}

	// 	App.Screenshot("If the app did not crash, then the test has passed.");
	// }
}
#endif