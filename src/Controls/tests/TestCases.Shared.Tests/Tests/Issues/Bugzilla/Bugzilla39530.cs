using System.Drawing;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
	[FailsOnIOSWhenRunningOnXamarinUITest]
	[FailsOnMacWhenRunningOnXamarinUITest]
	[FailsOnWindowsWhenRunningOnXamarinUITest("Fails finding the frame element. Investigate the cause.")]
	public void Bugzilla39530PanTest()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		// Got to wait for the element to be visible to the UI test framework, otherwise we get occasional 
		// index out of bounds exceptions if the query for the frame and its Rect run quickly enough
		App.WaitForElement("frame");
		Rectangle frameBounds = App.FindElement("frame").GetRect();
		App.Pan(frameBounds.CenterX(), frameBounds.Y + 10, frameBounds.X + 100, frameBounds.Y + 100);
		var result = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "Panning: Completed" + "']"));
		ClassicAssert.IsNotEmpty(result.Text);
	}

	/*
	[Test]
	[FailsOnIOSWhenRunningOnXamarinUITest]	
	[FailsOnMacWhenRunningOnXamarinUITest]
	public void Bugzilla39530PinchTest()
	{
		App.PinchToZoomIn("frame");
		App.WaitForElement(q => q.Marked("Pinching: Completed"));
	}

	[Test]
	[FailsOnIOSWhenRunningOnXamarinUITest]
	[FailsOnMacWhenRunningOnXamarinUITest]
	public void Bugzilla39530TapTest()
	{
		App.WaitForElement("frame");
		App.Tap("frame");
		App.WaitForElement("Taps: 1");
		App.Tap("frame");
		App.WaitForElement("Taps: 2");
	}
	*/
}
