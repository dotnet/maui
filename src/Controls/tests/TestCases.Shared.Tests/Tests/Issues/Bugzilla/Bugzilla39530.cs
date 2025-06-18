#if TEST_FAILS_ON_CATALYST //Pan is not working on the MacCatalyst.
using System.Drawing;
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

	[Test]
	public void Bugzilla39530PanTest()
	{
		App.WaitForElement("frameLabel");
		Rectangle frameBounds = App.FindElement("frameLabel").GetRect();
		App.Pan(frameBounds.CenterX(), frameBounds.Y + 10, frameBounds.X + 100, frameBounds.Y + 100);
		App.WaitForElement("Panning: Completed");
	}


	[Test]
	public void Bugzilla39530PinchTest()
	{
		//The PinchToZoomIn gesture doesn't work on the Frame for other platforms, so it should be applied to the children of the Frame instead.
#if ANDROID
		App.PinchToZoomIn("frame");
#else
		App.PinchToZoomIn("frameLabel");
#endif
		App.WaitForElement("Pinching: Completed");
	}

	[Test]
	public void Bugzilla39530TapTest()
	{
		App.WaitForElement("frameLabel");
		App.Tap("frameLabel");
		App.WaitForElement("Taps: 1");
		App.Tap("frameLabel");
		App.WaitForElement("Taps: 2");
	}

}
#endif