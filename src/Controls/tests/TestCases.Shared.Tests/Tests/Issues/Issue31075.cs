#if !WINDOWS // On Windows on hosted runners there is no app installed to capture images/videos
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31075 : _IssuesUITest
{
	public Issue31075(TestDevice device) : base(device)
	{
	}

	public override string Issue => "MediaPicker.CapturePhotoAsync / CaptureVideoAsync causes modal page to dismiss unexpectedly";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void MediaPickerShouldNotDismissModal()
	{
		// Open the modal page
		App.WaitForElement("OpenModalButton");
		App.Tap("OpenModalButton");

		// Verify modal opened
		App.WaitForElement("TestMediaPickerButton");
		App.WaitForElement("CloseModalButton");

		// Test MediaPicker (this would previously dismiss the modal on iOS)
		App.Tap("TestMediaPickerButton");

		// On iOS, a permission dialog might appear - we'll handle this gracefully
		try
		{
			// Wait a moment for any permission dialogs
			System.Threading.Thread.Sleep(2000);

			// Try to dismiss any system dialogs (camera permission, camera UI, etc.)
			// The exact behavior will depend on the iOS simulator/device state
			// Since we can't easily test the actual camera in UI tests, 
			// the important thing is that the modal page is still present
		}
		catch
		{
			// Ignore any exceptions from system UI interactions
		}

		// The critical test: verify the modal page is still present
		// If the bug exists, this element would not be found because the modal would be dismissed
		App.WaitForElement("CloseModalButton", "Modal should still be present after MediaPicker interaction");
		App.WaitForElement("TestMediaPickerButton", "Modal content should still be present");

		// Clean up by closing the modal properly
		App.Tap("CloseModalButton");

		// Verify we're back to the main page
		App.WaitForElement("OpenModalButton");
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public void VideoPickerShouldNotDismissModal()
	{
		// Open the modal page
		App.WaitForElement("OpenModalButton");
		App.Tap("OpenModalButton");

		// Verify modal opened
		App.WaitForElement("TestVideoPickerButton");
		App.WaitForElement("CloseModalButton");

		// Test Video MediaPicker (this would previously dismiss the modal on iOS)
		App.Tap("TestVideoPickerButton");

		// Similar to photo test - handle system UI gracefully
		try
		{
			System.Threading.Thread.Sleep(2000);
		}
		catch
		{
			// Ignore any exceptions from system UI interactions
		}

		// The critical test: verify the modal page is still present
		App.WaitForElement("CloseModalButton", "Modal should still be present after MediaPicker interaction");
		App.WaitForElement("TestVideoPickerButton", "Modal content should still be present");

		// Clean up by closing the modal properly
		App.Tap("CloseModalButton");

		// Verify we're back to the main page
		App.WaitForElement("OpenModalButton");
	}
}
#endif