using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Gestures)]
public class Bugzilla59863_2 : _IssuesUITest
{
	const string MixedTapBoxId = "mixedTapView";
	const string Singles = "singles(s)";
	const string Doubles = "double(s)";

	public Bugzilla59863_2(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TapGestureRecognizer extremely finicky2";

	[Test]
	[FailsOnIOS]
	public void DoubleTapWithMixedRecognizersShouldRegisterDoubleTap()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		App.WaitForElement(MixedTapBoxId);
		App.DoubleTap(MixedTapBoxId);
		
		var result = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + $"1 {Doubles} on {MixedTapBoxId}" + "']"));
		ClassicAssert.NotNull(result);
	}

	[Test]
	[FailsOnIOS]
	public void SingleTapWithMixedRecognizersShouldRegisterSingleTap()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		App.WaitForElement(MixedTapBoxId);
		App.Tap(MixedTapBoxId);

		var result = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + $"1 {Singles} on {MixedTapBoxId}" + "']"));
		ClassicAssert.NotNull(result);
	}
}