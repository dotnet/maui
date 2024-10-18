using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Gestures)]
public class Bugzilla59863_0 : _IssuesUITest
{
	const string SingleTapBoxId = "singleTapView";
	const string Singles = "singles(s)";

	public Bugzilla59863_0(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TapGestureRecognizer extremely finicky";

	[Test]
	[FailsOnIOS]
	public void TapsCountShouldMatch()
	{
		// Gonna add this test because we'd want to know if it _did_ start failing
		// But it doesn't really help much with this issue; UI test can't tap fast enough to demonstrate the 
		// problem we're trying to solve
		
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		int tapsToTest = 5;

		App.WaitForElement(SingleTapBoxId);

		for (int n = 0; n < tapsToTest; n++)
		{
			App.Tap(SingleTapBoxId);
		}

		var result = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + $"{tapsToTest} {Singles} on {SingleTapBoxId}" + "']"));
		ClassicAssert.NotNull(result);
	}

	[Test]
	[FailsOnIOS]
	public void DoubleTapWithOnlySingleTapRecognizerShouldRegisterTwoTaps()
	{
		if (App is not AppiumApp app2 || app2 is null || app2.Driver is null)
		{
			throw new InvalidOperationException("Cannot run test. Missing driver to run quick tap actions.");
		}

		App.WaitForElement(SingleTapBoxId);
		App.DoubleTap(SingleTapBoxId);

		var result = app2.Driver.FindElement(OpenQA.Selenium.By.XPath("//*[@text='" + $"2 {Singles} on {SingleTapBoxId}" + "']"));
		ClassicAssert.NotNull(result);
	}
}