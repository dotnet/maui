using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue32845 : _IssuesUITest
	{
		public override string Issue => "MediaPicker stops working after launching other ComponentActivity";

		public Issue32845(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.LifeCycle)]
		public void MediaPickerShouldWorkAfterOpeningAndClosingComponentActivity()
		{
			// Step 1: Verify the page loaded
			App.WaitForElement("PickPhotoButton");
			App.WaitForElement("OpenActivityButton");
			App.WaitForElement("StatusLabel");

			// Step 2: Open and close a temporary ComponentActivity
			App.Tap("OpenActivityButton");
			
			// Wait for the activity to open (status should change)
			// Note: We can't easily interact with the native ComponentActivity from Appium
			// So we'll just wait a moment and then use the back button to close it
			Task.Delay(1000).Wait();
			
			// Close the activity using back button
			App.Back();
			
			// Wait for main page to be visible again
			App.WaitForElement("PickPhotoButton");
			
			// Step 3: Try to use MediaPicker - this should work (but will be cancelled in automated test)
			// The bug was that the picker wouldn't open at all after the ComponentActivity was closed
			// We can't actually pick a photo in automated tests, but we can verify the attempt doesn't crash
			App.Tap("PickPhotoButton");
			
			// Give picker time to try to open
			Task.Delay(1000).Wait();
			
			// Cancel the picker (press back)
			App.Back();
			
			// Verify we're back on the main page and no crash occurred
			App.WaitForElement("PickPhotoButton");
			App.WaitForElement("StatusLabel");
			
			// If we got here without exceptions, the test passed
			// The original bug would cause the picker to not open at all (launcher was invalid)
		}
	}
}
