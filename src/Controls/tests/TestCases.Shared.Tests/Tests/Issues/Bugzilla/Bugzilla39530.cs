using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Gestures)]
public class Bugzilla39530 : _IssuesUITest
{
	public Bugzilla39530(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Frames do not handle pan or pinch gestures under AppCompat";

// TODO From Xamarin.UITest migration: does some advanced XamUITest operations
// Need to find Appium equivalents
// 	[Test]
// #if __MACOS__
// 	[Ignore("UITest.Desktop doesn't return empty NSView yet so it can't find the frame")]
// #endif
// 	[FailsOnIOS]
// 	public void Bugzilla39530PanTest()
// 	{
// 		// Got to wait for the element to be visible to the UI test framework, otherwise we get occasional 
// 		// index out of bounds exceptions if the query for the frame and its Rect run quickly enough
// 		App.WaitForElement(q => q.Marked("frame"));
// 		AppRect frameBounds = App.Query (q => q.Marked ("frame"))[0].Rect;
// 		App.Pan (new Drag (frameBounds, frameBounds.CenterX, frameBounds.Y + 10, frameBounds.X + 100, frameBounds.Y + 100, Drag.Direction.LeftToRight));

// 		App.WaitForElement (q => q.Marked ("Panning: Completed"));
// 	}

// 	[Test]
// #if __MACOS__
// 	[Ignore("UITest.Desktop doesn't return empty NSView yet so it can't find the frame")]
// #endif
// 	[FailsOnIOS]
// 	public void Bugzilla39530PinchTest()
// 	{
// 		App.PinchToZoomIn ("frame");
// 		App.WaitForElement(q => q.Marked("Pinching: Completed"));
// 	}

// 	[Test]
// #if __MACOS__
// 	[Ignore("UITest.Desktop doesn't return empty NSView yet so it can't find the frame")]
// #endif
// 	[FailsOnIOS]
// 	public void Bugzilla39530TapTest()
// 	{
// 		App.WaitForElement (q => q.Marked ("frame"));
// 		App.Tap ("frame");
// 		App.WaitForElement (q => q.Marked ("Taps: 1"));
// 		App.Tap ("frame");
// 		App.WaitForElement (q => q.Marked ("Taps: 2"));
// 	}
}
