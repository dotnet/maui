#if TEST_FAILS_ON_CATALYST  //The test fails on macOS Catalyst because the PinchToZoomIn does not working.                                                                                  
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla57515 : _IssuesUITest
{
	const string ZoomContainer = "zoomContainer";
	const string ZoomImage = "zoomImg";

	public Bugzilla57515(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "PinchGestureRecognizer not getting called on Android ";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void Bugzilla57515Test()
	{
		App.WaitForElement(ZoomImage);
		App.WaitForElement("1");
		App.PinchToZoomIn(ZoomContainer);
		App.WaitForNoElement("1");
	}
}
#endif