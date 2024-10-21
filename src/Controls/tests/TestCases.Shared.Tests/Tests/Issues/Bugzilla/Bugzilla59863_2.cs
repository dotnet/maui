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

	[Test]
	[FailsOnIOS]
	[FailsOnWindows]
	public void DoubleTapWithMixedRecognizersShouldRegisterDoubleTap()
	{
		RunningApp.WaitForElement(MixedTapBoxId);
		RunningApp.DoubleTap(MixedTapBoxId);
		RunningApp.WaitForElement($"1 {Doubles} on {MixedTapBoxId}");
	}

	[Test]
	[FailsOnIOS]
	[FailsOnWindows]
	public void SingleTapWithMixedRecognizersShouldRegisterSingleTap()
	{
		RunningApp.WaitForElement(MixedTapBoxId);
		RunningApp.Tap(MixedTapBoxId);
		RunningApp.WaitForElement($"1 {Singles} on {MixedTapBoxId}");
	}
}