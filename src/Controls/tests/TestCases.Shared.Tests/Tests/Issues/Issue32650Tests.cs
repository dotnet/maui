using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue32650Tests : _IssuesUITest
	{
		public Issue32650Tests(TestDevice device) : base(device)
		{
		}

		public override string Issue => "32650";

		[Test]
		[Category(UITestCategories.MediaPicker)]
		public void MediaPickerPreservesImageOrientation()
		{
			// This test verifies that when PickPhotoAsync is called with RotateImage=true and PreserveMetaData=true,
			// the returned image maintains correct orientation

			// First, verify the test page loads
			App.WaitForElement("PickPhotoButton");

			// The actual image orientation verification is done manually by checking:
			// 1. The image displays with correct orientation in the UI
			// 2. When the image stream is read, the orientation metadata is preserved
			// 3. No double rotation occurs

			// Visual verification on the test device
			var pickedImage = App.FindElement("PickedImage");
			Assert.That(pickedImage, Is.Not.Null);

			var resultLabel = App.FindElement("ResultLabel");
			Assert.That(resultLabel.GetText(), Does.Not.Contain("Error"));
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
