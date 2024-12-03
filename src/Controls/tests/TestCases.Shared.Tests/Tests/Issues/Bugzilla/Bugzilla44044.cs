#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla44044 : _IssuesUITest
{
	//string _btnToggleSwipe = "btnToggleSwipe";
	//string _btnDisplayAlert = "btnDisplayAlert";

	public Bugzilla44044(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TabbedPage steals swipe gestures";

	// [Test]
	// [Category(UITestCategories.TabbedPage)]
	// public void Bugzilla44044Test()
	// {
	// 	App.WaitForElement(_btnToggleSwipe);

	// 	App.SwipeRightToLeft();
	// 	App.WaitForNoElement(_btnToggleSwipe);
	// 	App.WaitForElement(_btnDisplayAlert);

	// 	App.SwipeLeftToRight();
	// 	App.WaitForNoElement(_btnDisplayAlert);
	// 	App.WaitForElement(_btnToggleSwipe);

	// 	App.Tap(_btnToggleSwipe);
	// 	App.SwipeRightToLeft();
	// 	App.WaitForNoElement(_btnDisplayAlert);
	// 	App.WaitForElement(_btnToggleSwipe);
	// }
}
#endif