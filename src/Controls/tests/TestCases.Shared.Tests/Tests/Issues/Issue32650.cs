using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue32650 : _IssuesUITest
	{
		public Issue32650(TestDevice device) : base(device)
		{
		}

		public override string Issue => "MediaPicker.PickPhotosAsync does not preserve image orientation";

		[Test]
		[Category(UITestCategories.MediaPicker)]
		public void MediaPickerPreservesImageOrientation()
		{
			// This test verifies that the test page loads correctly with all UI elements present.
			// The actual image orientation verification requires device interaction (picking a photo)
			// which cannot be fully automated. This test confirms the page and controls are accessible.

			App.WaitForElement("PickPhotoButton");
			App.WaitForElement("PickMultiplePhotosButton");
			App.WaitForElement("CapturePhotoButton");

			var pickedImage = App.FindElement("PickedImage");
			Assert.That(pickedImage, Is.Not.Null);

			var resultLabel = App.FindElement("ResultLabel");
			Assert.That(resultLabel, Is.Not.Null);
		}

		[Test]
		[Category(UITestCategories.MediaPicker)]
		public void MediaPickerPickPhotoButton()
		{
			App.WaitForElement("PickPhotoButton");
			App.Tap("PickPhotoButton");

			// Wait for the result (this will depend on user action in real testing)
			// The result label should show either a filename or an error
			// In automated testing, we can verify the button exists and is tappable
		}

		[Test]
		[Category(UITestCategories.MediaPicker)]
		public void MediaPickerMultiplePhotosButton()
		{
			App.WaitForElement("PickMultiplePhotosButton");
			App.Tap("PickMultiplePhotosButton");

			// Verify the button is tappable
		}

		[Test]
		[Category(UITestCategories.MediaPicker)]
		public void MediaPickerCapturePhotoButton()
		{
			// Check if capture is supported
			var captureButton = App.FindElement("CapturePhotoButton");
			Assert.That(captureButton, Is.Not.Null);

			// The button should be visible and tappable (though capture requires device interaction)
			App.Tap("CapturePhotoButton");
		}
	}
}
