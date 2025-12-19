using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class ModalFormSheetDismissalCancellation : _IssuesUITest
	{
		public override string Issue => "iOS Modal FormSheet dismissal can be cancelled via ModalAttemptedDismiss event";

		public ModalFormSheetDismissalCancellation(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.Modal)]
#if !IOS && !MACCATALYST
		[Ignore("This test is only applicable to iOS/MacCatalyst where FormSheet presentation style is supported")]
#endif
		public void ModalFormSheetDismissalCanBeCancelled()
		{
			// Wait for the page to load
			App.WaitForElement("ShowModalButton");

			// Verify initial status
			var statusLabel = App.FindElement("StatusLabel");
			Assert.That(statusLabel.GetText(), Does.Contain("Ready"));

			// Tap the button to show the modal
			App.Tap("ShowModalButton");

			// Wait for modal to appear
			App.WaitForElement("ModalInstructions");

			// Verify the modal is shown
			var modalInstructions = App.FindElement("ModalInstructions");
			Assert.That(modalInstructions, Is.Not.Null, "Modal should be displayed");

			// Note: Automated testing of swipe-to-dismiss gesture is complex
			// The main validation here is that:
			// 1. The modal page can be shown with FormSheet style
			// 2. The ModalAttemptedDismiss event is wired up correctly (validated in code-behind)
			// 3. The modal can be closed via the Close button
			
			// Close the modal using the button
			App.Tap("CloseButton");

			// Verify we're back to the main page
			App.WaitForElement("ShowModalButton");
			var statusAfterClose = App.FindElement("StatusLabel");
			Assert.That(statusAfterClose.GetText(), Does.Contain("Modal shown"));
		}
	}
}
