#if TEST_FAILS_ON_WINDOWS   //BoxView automation ID isn't working on the Windows platform, causing a TimeoutException.                                                                                             
using NUnit.Framework;
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

	[Test, Order(2)]
	public void DoubleTapWithMixedRecognizersShouldRegisterDoubleTap()
	{
		App.WaitForElement(MixedTapBoxId);
		App.DoubleTap(MixedTapBoxId);
		App.WaitForElement($"1 {Doubles} on {MixedTapBoxId}");
	}

	[Test, Order(1)]
	public void SingleTapWithMixedRecognizersShouldRegisterSingleTap()
	{
		App.WaitForElement(MixedTapBoxId);
		App.Tap(MixedTapBoxId);
		App.WaitForElement($"1 {Singles} on {MixedTapBoxId}");
	}
}
#endif