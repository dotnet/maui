using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class IssueEntryTextChangedCrash : _IssuesUITest
	{
		const string EntryId = "TestEntry";

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
			
			// Test should complete without crashing
		}
	}
}
