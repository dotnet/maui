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
	const string ChildActivityOverlapButton = "ChildActivityOverlapButton";
	const string ChildActivityFinishWhilePickingButton = "ChildActivityFinishWhilePickingButton";
	const string ChildActivityResultLabel = "ChildActivityResultLabel";
	const string PhotoPickerAvailabilityLabel = "PhotoPickerAvailabilityLabel";
	const string StatusLabel = "StatusLabel";

	[Test]
	[Category(UITestCategories.Essentials)]
	public void PickPhotosAsyncShouldReturnFromChildActivity()
	{
		OpenChildActivityAndRequirePhotoPicker();

		// Tap Pick Photos — calls MediaPicker.PickPhotosAsync() from the child activity.
		// Before the fix the ActivityResultLauncher was never registered for child activities
		// (the guard in ActivityForResultRequest.Register() blocked it), so the task hung
		// indefinitely and the result label stayed on "Picking...".
		App.Tap(ChildActivityPickButton);
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

	[Test]
	[Category(UITestCategories.Essentials)]
	public void OverlappingPhotoPickerRequestsAreRejected()
	{
		OpenChildActivityAndRequirePhotoPicker();

		App.Tap(ChildActivityOverlapButton);
		App.Back();

		var rejected = App.WaitForTextToBePresentInElement(
			ChildActivityResultLabel,
			"Overlap Rejected",
			timeout: TimeSpan.FromSeconds(120));

		Assert.That(rejected, Is.True,
			"A second request must be rejected while the first activity-result callback is pending.");
		Assert.That(App.FindElement(ChildActivityResultLabel).GetText(), Does.Not.Contain("Error"));

		App.Back();
		App.WaitForElement(OpenChildActivityButton);
	}

	[Test]
	[Category(UITestCategories.Essentials)]
	public void FinishingLaunchingActivityCancelsPendingPhotoPicker()
	{
		OpenChildActivityAndRequirePhotoPicker();

		App.Tap(ChildActivityFinishWhilePickingButton);
		App.Back();

		App.WaitForElement(StatusLabel);
		var cancelled = App.WaitForTextToBePresentInElement(
			StatusLabel,
			"Launch activity cancelled",
			timeout: TimeSpan.FromSeconds(120));

		var statusText = App.FindElement(StatusLabel).GetText();
		Assert.That(cancelled, Is.True,
			$"Destroying the launching activity must cancel its pending picker request. Actual status: '{statusText}'.");
		Assert.That(statusText, Does.Not.Contain("Error"));
		Assert.That(statusText, Does.Not.Contain("unexpectedly"));
	}

	void OpenChildActivityAndRequirePhotoPicker()
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

		App.WaitForElement(OpenChildActivityButton);
		App.Tap(OpenChildActivityButton);

		App.WaitForElement(ChildActivityPickButton);
		App.WaitForElement(ChildActivityResultLabel);
		var availability = App.WaitForElement(PhotoPickerAvailabilityLabel).GetText()
			?? throw new AssertionException("Photo picker availability label did not expose text.");
		if (availability.Contains("Unavailable", StringComparison.Ordinal))
			Assert.Ignore("AndroidX Photo Picker is unavailable on this device; the changed launcher path is not active.");

		Assert.That(availability, Is.EqualTo("Photo Picker: Available"),
			"The regression test must exercise the AndroidX Photo Picker launcher path changed by this PR.");
	}
}
#endif