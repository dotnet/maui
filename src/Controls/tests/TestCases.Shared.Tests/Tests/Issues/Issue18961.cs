#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18961 : _IssuesUITest
	{
		const string ScrollButton = "ScrollButton";
		const string LastEntry = "TestEntry20";

		public Issue18961(TestDevice device) : base(device) { }

		public override string Issue => "Modal Page margin correct after Keyboard opens";

		[Test]
		[Category(UITestCategories.Layout)]
		public async Task ModalPageMarginCorrectAfterKeyboardOpens()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Ensure that the keyboard is closed before we start.
			App.DismissKeyboard();

			// 2. Scroll to the end.
			App.WaitForElement(ScrollButton);
			App.Click(ScrollButton);
			await Task.Delay(1000); // Wait for the scroll animation to finish.
			App.ScrollDown("TestScrollView"); // Ensure that we are at the end of the scroll.

			// 3. Get initial position of the Entry before keyboard opens
			App.WaitForElement(LastEntry);
			var initialRect = App.FindElement(LastEntry).GetRect();

			// 4. Focus latest Entry and enter text
			App.EnterText(LastEntry, "test");
			App.Click(LastEntry);
			await Task.Delay(1000);

			// 5. Verify the Entry is still visible (not covered by keyboard)
			// The Entry should be repositioned above the keyboard
			var entryWithKeyboard = App.FindElement(LastEntry).GetRect();
			Assert.That(entryWithKeyboard.Y, Is.LessThan(initialRect.Y),
				"Entry should be moved up when keyboard appears to remain visible");

			// 6. Close the keyboard to see if sizes adjust back.
			App.DismissKeyboard();
			await Task.Delay(1000);

			// 7. Verify the latest Entry text was preserved.
			var text = App.FindElement(LastEntry).GetText();
			Assert.That(text, Is.EqualTo("test"));

			// 8. Verify Entry returns to approximately its original position after keyboard dismissal
			var finalRect = App.FindElement(LastEntry).GetRect();
			Assert.That(Math.Abs(finalRect.Y - initialRect.Y), Is.LessThan(50),
				"Entry should return close to its original position after keyboard dismissal");
		}
	}
}
#endif