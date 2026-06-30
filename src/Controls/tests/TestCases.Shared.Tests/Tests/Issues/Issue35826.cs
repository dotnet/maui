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
		// TaskCompletionSource is never resolved. With the fix it updates promptly to
		// the expected cancellation state. Error indicates a launcher/ownership failure
		// or another exception path and must fail this regression.
		var returned = App.WaitForTextToBePresentInElement(ChildActivityResultLabel, "Cancelled",
			timeout: TimeSpan.FromSeconds(120));

		var resultText = App.FindElement(ChildActivityResultLabel).GetText();

		Assert.That(returned, Is.True,
			$"PickPhotosAsync must return from a child activity as a cancellation result after backing out of the picker. " +
			$"Actual result label: '{resultText}'. " +
			$"If this fails the result label is still showing 'Picking...' after 120 seconds or an exception path was hit.");

		Assert.That(resultText, Does.Not.Contain("Picking"),
			"PickPhotosAsync must not hang in a child activity.");

		Assert.That(resultText, Does.Not.Contain("Error"),
			$"PickPhotosAsync should cancel cleanly when backing out of the picker, not surface an exception. Actual result label: '{resultText}'.");

		// Return to the host page
		App.Back();
		App.WaitForElement(OpenChildActivityButton);
	}
}
#endif