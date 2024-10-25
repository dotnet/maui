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
//		RunningApp.WaitForElement(SwipeGestureSuccessId);
//		RunningApp.SwipeLeftToRight(SwipeGestureSuccessId);
//		RunningApp.WaitForElement(SwipeGestureSuccess);
//	}

//	[Test]
//	[Category(UITestCategories.TableView)]
//	public void TableViewScroll()
//	{
//		TapInFlyout(TableViewTitle);
//		RunningApp.WaitForElement(TableViewId);

//		RunningApp.ScrollDownTo("entry30", TableViewId, ScrollStrategy.Gesture, swipePercentage: 0.20, timeout: TimeSpan.FromMinutes(1));
//	}

//	[Test]
//	[Category(UITestCategories.ListView)]
//	public void ListViewScroll()
//	{
//		TapInFlyout(ListViewTitle);
//		RunningApp.WaitForElement(ListViewId);
//		RunningApp.ScrollDownTo("30 Entry", ListViewId, ScrollStrategy.Gesture, swipePercentage: 0.20, timeout: TimeSpan.FromMinutes(1));
//	}

//#if ANDROID
//	[Test]
//	[Category(UITestCategories.CustomRenderers)]
//	[FailsOnAndroid]
//	public void TouchListener()
//	{
//		TapInFlyout(TouchListenerTitle);
//		RunningApp.WaitForElement(TouchListenerSuccessId);
//		RunningApp.SwipeLeftToRight(TouchListenerSuccessId);
//		RunningApp.WaitForElement(TouchListenerSuccess);
//	}
//#endif
}