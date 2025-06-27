using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class ToolbarUITests : _ViewUITests
	{
		const string ToolbarIsVisibleTest = "Toolbar IsVisible Property Test";

		public ToolbarUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(ToolbarIsVisibleTest);
		}

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void ToolbarItemsIsVisibleProperty()
		{
			var testPage = GoToTestPage();

			// Initially, only the visible toolbar item should be visible
			// Hidden item should not be visible
			var statusLabel = testPage.WaitForElement("StatusLabel");
			ClassicAssert.AreEqual("Status: Initial state", statusLabel.GetText());

			// Verify the hidden item status shows it's hidden
			var hiddenItemStatusLabel = testPage.WaitForElement("HiddenItemStatusLabel");
			ClassicAssert.AreEqual("Hidden Item Visible: False", hiddenItemStatusLabel.GetText());

			// Verify the secondary hidden item status shows it's hidden
			var secondaryHiddenStatusLabel = testPage.WaitForElement("SecondaryHiddenStatusLabel");
			ClassicAssert.AreEqual("Secondary Hidden Item Visible: False", secondaryHiddenStatusLabel.GetText());

			// Toggle the hidden item visibility
			var toggleHiddenButton = testPage.WaitForElement("ToggleHiddenButton");
			toggleHiddenButton.Click();

			// Verify the status updated
			statusLabel = testPage.WaitForElement("StatusLabel");
			ClassicAssert.AreEqual("Status: Hidden item is now visible", statusLabel.GetText());

			var hiddenItemStatusAfterToggle = testPage.WaitForElement("HiddenItemStatusLabel");
			ClassicAssert.AreEqual("Hidden Item Visible: True", hiddenItemStatusAfterToggle.GetText());

			// Toggle it back to hidden
			toggleHiddenButton.Click();
			statusLabel = testPage.WaitForElement("StatusLabel");
			ClassicAssert.AreEqual("Status: Hidden item is now hidden", statusLabel.GetText());

			var hiddenItemStatusAfterHide = testPage.WaitForElement("HiddenItemStatusLabel");
			ClassicAssert.AreEqual("Hidden Item Visible: False", hiddenItemStatusAfterHide.GetText());
		}

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void ToolbarItemsToggleAllVisibility()
		{
			var testPage = GoToTestPage();

			// Toggle all items off
			var toggleAllButton = testPage.WaitForElement("ToggleAllButton");
			toggleAllButton.Click();

			var statusLabel = testPage.WaitForElement("StatusLabel");
			ClassicAssert.AreEqual("Status: All items are now hidden", statusLabel.GetText());

			var hiddenItemStatusLabel = testPage.WaitForElement("HiddenItemStatusLabel");
			ClassicAssert.AreEqual("Hidden Item Visible: False", hiddenItemStatusLabel.GetText());

			// Toggle all items back on
			toggleAllButton.Click();
			statusLabel = testPage.WaitForElement("StatusLabel");
			ClassicAssert.AreEqual("Status: All items are now visible", statusLabel.GetText());

			var hiddenItemStatusAfterShow = testPage.WaitForElement("HiddenItemStatusLabel");
			ClassicAssert.AreEqual("Hidden Item Visible: True", hiddenItemStatusAfterShow.GetText());
		}

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void SecondaryToolbarItemsIsVisibleProperty()
		{
			var testPage = GoToTestPage();

			// Initially, the secondary hidden item should be hidden
			var secondaryHiddenStatusLabel = testPage.WaitForElement("SecondaryHiddenStatusLabel");
			ClassicAssert.AreEqual("Secondary Hidden Item Visible: False", secondaryHiddenStatusLabel.GetText());

			// Toggle the secondary hidden item visibility
			var toggleSecondaryButton = testPage.WaitForElement("ToggleSecondaryButton");
			toggleSecondaryButton.Click();

			// Verify the status updated
			var statusLabel = testPage.WaitForElement("StatusLabel");
			ClassicAssert.AreEqual("Status: Secondary hidden item is now visible", statusLabel.GetText());

			var secondaryHiddenStatusAfterToggle = testPage.WaitForElement("SecondaryHiddenStatusLabel");
			ClassicAssert.AreEqual("Secondary Hidden Item Visible: True", secondaryHiddenStatusAfterToggle.GetText());

			// Toggle it back to hidden
			toggleSecondaryButton.Click();
			statusLabel = testPage.WaitForElement("StatusLabel");
			ClassicAssert.AreEqual("Status: Secondary hidden item is now hidden", statusLabel.GetText());

			var secondaryHiddenStatusAfterHide = testPage.WaitForElement("SecondaryHiddenStatusLabel");
			ClassicAssert.AreEqual("Secondary Hidden Item Visible: False", secondaryHiddenStatusAfterHide.GetText());
		}

		UITestElementQuery GoToTestPage()
		{
			var scrollView = App.WaitForElement("VerticalScrollBarDownBtn").First();
			return scrollView;
		}
	}
}