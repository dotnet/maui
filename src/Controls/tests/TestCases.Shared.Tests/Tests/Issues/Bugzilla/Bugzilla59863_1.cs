#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
using NUnit.Framework;
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
	[FailsOnIOSWhenRunningOnXamarinUITest]
	public void SingleTapWithOnlyDoubleTapRecognizerShouldRegisterNothing()
	{
		App.WaitForElement(DoubleTapBoxId);
		App.Tap(DoubleTapBoxId);
		App.WaitForElement($"0 {Doubles} on {DoubleTapBoxId}");
	}

	[Test]
	[FailsOnIOSWhenRunningOnXamarinUITest]
	public void DoubleTapWithOnlyDoubleTapRecognizerShouldRegisterOneDoubleTap()
	{
		App.WaitForElement(DoubleTapBoxId);
		App.DoubleTap(DoubleTapBoxId);
		App.WaitForElement($"1 {Doubles} on {DoubleTapBoxId}");
	}
}
#endif