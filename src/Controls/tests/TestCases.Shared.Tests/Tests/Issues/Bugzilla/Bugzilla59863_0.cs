#if TEST_FAILS_ON_WINDOWS  //BoxView automation ID isn't working on the Windows platform, causing a TimeoutException.
using NUnit.Framework;
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

	[Test, Order(2)]
	public void TapsCountShouldMatch()
	{
		// Gonna add this test because we'd want to know if it _did_ start failing
		// 	But it doesn't really help much with this issue; UI test can't tap fast enough to demonstrate the 
		// 	problem we're trying to solve

		int tapsToTest = 5;

		App.WaitForElement(SingleTapBoxId);

		for (int n = 0; n < tapsToTest; n++)
		{
			App.Tap(SingleTapBoxId);
		}

		App.WaitForElement($"{tapsToTest + 2} {Singles} on {SingleTapBoxId}");
	}

	[Test, Order(1)]
	public void DoubleTapWithOnlySingleTapRecognizerShouldRegisterTwoTaps()
	{
		App.WaitForElement(SingleTapBoxId);
		App.DoubleTap(SingleTapBoxId);
		App.WaitForElement($"2 {Singles} on {SingleTapBoxId}");
	}
}
#endif
