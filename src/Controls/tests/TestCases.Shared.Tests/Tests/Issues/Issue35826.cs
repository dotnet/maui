#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35826 : _IssuesUITest
{
	public Issue35826(TestDevice device) : base(device) { }

	public override string Issue => "PickPhotosAsync hangs when called from a child activity";

	const string OpenChildActivityButton = "OpenChildActivityButton";
	const string ChildActivityPickButton = "ChildActivityPickButton";
	const string ChildActivityResultLabel = "ChildActivityResultLabel";

	[Test]
	[Category(UITestCategories.Essentials)]
	public void PickPhotosAsyncShouldReturnFromChildActivity()
	{
		// This regression only manifests on Android API 36, where the ActivityResultLauncher
		// ownership rules are enforced strictly enough that using the wrong activity's launcher
		// causes the result to never be delivered, hanging the task indefinitely.
		if (App is AppiumApp appiumApp)
		{
			var apiLevel = (long?)appiumApp.Driver.Capabilities.GetCapability("deviceApiLevel") ?? 0;
			if (apiLevel < 36)
			{
				Assert.Ignore($"Issue #35826 only manifests on Android API 36+. Current device API: {apiLevel}.");
			}
		}

		// Verify the host page loaded
		App.WaitForElement(OpenChildActivityButton);

		// Open the child (non-MAUI AppCompatActivity)
		App.Tap(OpenChildActivityButton);

		// Verify the child activity's UI is visible
		App.WaitForElement(ChildActivityPickButton);
		App.WaitForElement(ChildActivityResultLabel);

		// Tap Pick Photos — calls MediaPicker.PickPhotosAsync() from the child activity.
		// Before the fix the ActivityResultLauncher was never registered for child activities
		// (the guard in ActivityForResultRequest.Register() blocked it), so the task hung
		// indefinitely and the result label stayed on "Picking...".
		App.Tap(ChildActivityPickButton);

		// Cancel the system photo picker by pressing Back.
		// After the fix each activity has its own launcher entry so the result is delivered.
		App.Back();

		// If the bug is present WaitForTextToBePresentInElement times out because the
		// TaskCompletionSource is never resolved. With the fix it updates promptly.
		// Also accept "Error" as a valid outcome: what matters is that the call returns
		// (doesn't hang), not the specific result — emulators may not have a photo picker.
		var returned = App.WaitForTextToBePresentInElement(ChildActivityResultLabel, "Cancelled",
			timeout: TimeSpan.FromSeconds(120)) ||
			App.WaitForTextToBePresentInElement(ChildActivityResultLabel, "Error",
			timeout: TimeSpan.FromSeconds(5));

		var resultText = App.FindElement(ChildActivityResultLabel).GetText();

		Assert.That(returned, Is.True,
			$"PickPhotosAsync must return from a child activity (not hang indefinitely). " +
			$"Actual result label: '{resultText}'. " +
			$"If this fails the result label is still showing 'Picking...' after 120 seconds.");

		Assert.That(resultText, Does.Not.Contain("Picking"),
			"PickPhotosAsync must not hang in a child activity.");

		// Return to the host page
		App.Back();
		App.WaitForElement(OpenChildActivityButton);
	}
}
#endif