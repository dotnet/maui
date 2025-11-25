#if IOS // FormSheet presentation style only exists on iOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue6994FormSheet : _IssuesUITest
	{
		public override string Issue => "[iOS] Reusing the same page for formsheet Modal causes measuring issues";

		public Issue6994FormSheet(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Page)]
		public void ReusedFormSheetModalMaintainsConsistentDimensions()
		{
			// Wait for the main page to load
			App.WaitForElement("OpenFormSheetButton");

			// First modal open
			App.Tap("OpenFormSheetButton");
			App.WaitForElement("ModalDimensionsLabel");

			// Wait for dimensions to be displayed (format: "540 x 620")
			App.WaitForTextToBePresentInElement("ModalDimensionsLabel", " x ");

			// Get the first dimensions
			var firstDimensions = App.FindElement("ModalDimensionsLabel").GetText();
			Assert.That(firstDimensions, Is.Not.Null.And.Not.Empty, "First dimensions should be displayed");

			// Close the modal
			App.Tap("CloseModalButton");
			App.WaitForElement("OpenFormSheetButton");

			// Second modal open (reusing the same page instance)
			App.Tap("OpenFormSheetButton");
			App.WaitForElement("ModalDimensionsLabel");

			// Wait for dimensions to be displayed
			App.WaitForTextToBePresentInElement("ModalDimensionsLabel", " x ");

			// Get the second dimensions
			var secondDimensions = App.FindElement("ModalDimensionsLabel").GetText();
			Assert.That(secondDimensions, Is.Not.Null.And.Not.Empty, "Second dimensions should be displayed");

			// Verify dimensions are consistent
			Assert.That(secondDimensions, Is.EqualTo(firstDimensions),
				$"Modal dimensions should remain consistent. First: {firstDimensions}, Second: {secondDimensions}");

			// Close the modal
			App.Tap("CloseModalButton");
			App.WaitForElement("OpenFormSheetButton");

			// Third modal open (one more time to ensure stability)
			App.Tap("OpenFormSheetButton");
			App.WaitForElement("ModalDimensionsLabel");

			// Wait for dimensions to be displayed
			App.WaitForTextToBePresentInElement("ModalDimensionsLabel", " x ");

			// Get the third dimensions
			var thirdDimensions = App.FindElement("ModalDimensionsLabel").GetText();
			Assert.That(thirdDimensions, Is.Not.Null.And.Not.Empty, "Third dimensions should be displayed");

			// Verify dimensions are still consistent
			Assert.That(thirdDimensions, Is.EqualTo(firstDimensions),
				$"Modal dimensions should remain consistent after third open. First: {firstDimensions}, Third: {thirdDimensions}");

			// Close the modal to clean up
			App.Tap("CloseModalButton");
			App.WaitForElement("OpenFormSheetButton");
		}
	}
}
#endif
