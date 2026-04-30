#if IOS || ANDROID //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34273 : _IssuesUITest
{
	public Issue34273(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Rotating simulator makes Stepper and Label overlap";

	[Test]
	[Category(UITestCategories.Editor)]
	public void EditorNoOverlapAfterRotateToLandscape()
	{
		App.WaitForElement("TestEditor");

		App.SetOrientationLandscape();
		// Wait for the layout to re-render after orientation change before verifying
		App.WaitForElement("TestEditor");

#if ANDROID
		VerifyScreenshot(cropLeft:125, retryTimeout: TimeSpan.FromSeconds(2));
#else
		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
#endif
	}

	[Test]
	[Category(UITestCategories.Editor)]
	public void EditorNoOverlapAfterRotateToPortrait()
	{
		App.WaitForElement("TestEditor");

		App.SetOrientationLandscape(); // rotate to landscape first to trigger the bug scenario
		// Wait for the layout to re-render after orientation change before continuing
		App.WaitForElement("TestEditor");

		App.SetOrientationPortrait(); // rotate back to portrait to verify layout recovers
		// Wait for the layout to re-render after orientation change before verifying
		App.WaitForElement("TestEditor");

		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif
