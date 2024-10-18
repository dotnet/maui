using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Gestures)]
public class Bugzilla59863_1 : _IssuesUITest
{
	const string DoubleTapBoxId = "doubleTapView";
	const string Doubles = "double(s)";

	public Bugzilla59863_1(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TapGestureRecognizer extremely finicky1";

	[Test]
	[FailsOnIOS]
	public void SingleTapWithOnlyDoubleTapRecognizerShouldRegisterNothing()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		App.WaitForElement(DoubleTapBoxId);
		App.Tap(DoubleTapBoxId);

		var result = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + $"0 {Doubles} on {DoubleTapBoxId}" + "']"));
		ClassicAssert.NotNull(result);
	}

	[Test]
	[FailsOnIOS]
	public void DoubleTapWithOnlyDoubleTapRecognizerShouldRegisterOneDoubleTap()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		App.WaitForElement(DoubleTapBoxId);
		App.DoubleTap(DoubleTapBoxId);

		var result = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + $"1 {Doubles} on {DoubleTapBoxId}" + "']"));
		ClassicAssert.NotNull(result);
	}
}