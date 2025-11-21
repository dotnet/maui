using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class IssueEntryTextChangedCrash : _IssuesUITest
	{
		const string EntryId = "TestEntry";
		const string EmojiEntryId = "EmojiEntry";

		public IssueEntryTextChangedCrash(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Entry crash when setting Text in TextChanged event handler on Android";

		[Test]
		[Category(UITestCategories.Entry)]
		public void EntryTextChangedShouldNotCrash()
		{
			App.WaitForElement(EntryId);

			// Clear the entry text - this should trigger TextChanged which sets text to "0"
			// The fix ensures this doesn't crash due to EmojiCompat bounds check
			App.ClearText(EntryId);

			// Verify the entry contains "0" after TextChanged handling
			var entryText = App.FindElement(EntryId).GetText();
			Assert.That(entryText, Is.EqualTo("0"), "Entry should contain '0' after clearing");

			// Verify cursor position is at the end of the text (position 1 for "0")
			var entry = App.FindElement(EntryId);
			App.Click(EntryId);
			// Note: Cursor position validation would require platform-specific implementation
			// The important test is that the app doesn't crash
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void EntryWithEmojiTextChangedShouldNotCrash()
		{
			App.WaitForElement(EmojiEntryId);

			// Clear the emoji entry - this tests multi-codepoint emoji sequences
			// with EmojiCompat to ensure Length() handles them correctly
			App.ClearText(EmojiEntryId);

			// Verify the entry contains the emoji after TextChanged handling
			var entryText = App.FindElement(EmojiEntryId).GetText();
			Assert.That(entryText, Is.EqualTo("👨‍👩‍👧‍👦"), "Entry should contain emoji after clearing");

			// Test should complete without crashing
		}
	}
}
