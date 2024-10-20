using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Gestures)]
public class Bugzilla59863_2 : _IssuesUITest
{
	public Bugzilla59863_2(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TapGestureRecognizer extremely finicky2";

	// [Test]
	// [FailsOnIOS]
	// public void DoubleTapWithMixedRecognizersShouldRegisterDoubleTap()
	// {
	// 	App.WaitForElement(MixedTapBoxId);
	// 	App.DoubleTap(MixedTapBoxId);

	// 	App.WaitForElement($"1 {Doubles} on {MixedTapBoxId}");
	// }

	// [Test]
	// [FailsOnIOS]
	// public void SingleTapWithMixedRecognizersShouldRegisterSingleTap()
	// {
	// 	App.WaitForElement(MixedTapBoxId);
	// 	App.Tap(MixedTapBoxId);

	// 	App.WaitForElement($"1 {Singles} on {MixedTapBoxId}");
	// }
}