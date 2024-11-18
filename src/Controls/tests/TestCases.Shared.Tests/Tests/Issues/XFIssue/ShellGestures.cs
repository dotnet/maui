using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class ShellGestures : _IssuesUITest
{
	public ShellGestures(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Gestures Test";

	//	[Test]
	//	[Category(UITestCategories.Gestures)]
	//	public void SwipeGesture()
	//	{
	//		TapInFlyout(SwipeTitle, usingSwipe: true);
	//		App.WaitForElement(SwipeGestureSuccessId);
	//		App.SwipeLeftToRight(SwipeGestureSuccessId);
	//		App.WaitForElement(SwipeGestureSuccess);
	//	}

	//	[Test]
	//	[Category(UITestCategories.TableView)]
	//	public void TableViewScroll()
	//	{
	//		TapInFlyout(TableViewTitle);
	//		App.WaitForElement(TableViewId);

	//		App.ScrollDownTo("entry30", TableViewId, ScrollStrategy.Gesture, swipePercentage: 0.20, timeout: TimeSpan.FromMinutes(1));
	//	}

	//	[Test]
	//	[Category(UITestCategories.ListView)]
	//	public void ListViewScroll()
	//	{
	//		TapInFlyout(ListViewTitle);
	//		App.WaitForElement(ListViewId);
	//		App.ScrollDownTo("30 Entry", ListViewId, ScrollStrategy.Gesture, swipePercentage: 0.20, timeout: TimeSpan.FromMinutes(1));
	//	}

	//#if ANDROID
	//	[Test]
	//	[Category(UITestCategories.CustomRenderers)]
	//	[FailsOnAndroidWhenRunningOnXamarinUITest]
	//	public void TouchListener()
	//	{
	//		TapInFlyout(TouchListenerTitle);
	//		App.WaitForElement(TouchListenerSuccessId);
	//		App.SwipeLeftToRight(TouchListenerSuccessId);
	//		App.WaitForElement(TouchListenerSuccess);
	//	}
	//#endif
}