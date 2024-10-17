using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla57515 : _IssuesUITest
{
	public Bugzilla57515(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "PinchGestureRecognizer not getting called on Android ";

	// [Test]
	// [Category(UITestCategories.Gestures)]
	// [FailsOnIOS]
	// public void Bugzilla57515Test()
	// {
	// 	RunningApp.WaitForElement(ZoomContainer);
	// 	RunningApp.WaitForElement("1");
	// 	RunningApp.PinchToZoomIn(ZoomContainer);
	// 	RunningApp.WaitForNoElement("1"); // The scale should have changed during the zoom
	// }
}