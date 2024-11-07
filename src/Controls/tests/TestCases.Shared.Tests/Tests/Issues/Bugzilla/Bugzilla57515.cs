#if ANDROID || IOS // TODO: Cannot find the element ZoomContainer on Desktop. Investigate the reason.
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla57515 : _IssuesUITest
{
	const string ZoomContainer = "zoomContainer";

	public Bugzilla57515(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "PinchGestureRecognizer not getting called on Android ";

	[Test]
	[Category(UITestCategories.Gestures)]
	[FailsOnIOSWhenRunningOnXamarinUITest]
	public void Bugzilla57515Test()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		App.WaitForElement(ZoomContainer);
		var zoomScale1 = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + "1" + "']"));
		ClassicAssert.AreEqual("1", zoomScale1.Text);
		App.PinchToZoomIn(ZoomContainer);
		var elements = app2.Driver.FindElements(OpenQA.Selenium.By.XPath("//*[@text='" + "1" + "']"));
		ClassicAssert.AreEqual(0, elements.Count); // The scale should have changed during the zoom
	}
}
#endif
