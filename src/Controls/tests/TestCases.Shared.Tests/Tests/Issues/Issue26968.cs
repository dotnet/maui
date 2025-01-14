#if TEST_FAILS_ON_WINDOWS // NoCaret Controls not implemented on Windows.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue26968 : _IssuesUITest
	{
		public override string Issue => "Need reliable validation support to ensure the KeyboardType in Appium";

		public Issue26968(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.Editor)]
		public void VerifyNoCaretEditor()
		{
			App.WaitForElement("TestEditor");
			App.Tap("TestEditor");
			App.EnterText("TestEditor", "Test");

			App.DismissKeyboard();
			QueryUntilKeyboardNotPresent(App.IsKeyboardShown);

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyNoCaretEntry()
		{
			App.WaitForElement("TestEntry");
			App.Tap("TestEntry");
			App.EnterText("TestEntry", "Test");

			App.DismissKeyboard();
			QueryUntilKeyboardNotPresent(App.IsKeyboardShown);

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.SearchBar)]
		public void VerifyNoCaretSearchBar()
		{
			App.WaitForElement("TestSearchBar");
			App.Tap("TestSearchBar");
			App.EnterText("TestSearchBar", "Test");

			App.DismissKeyboard();
			QueryUntilKeyboardNotPresent(App.IsKeyboardShown);

			VerifyScreenshot();
		}
		
		static T QueryUntilKeyboardNotPresent<T>(
			Func<T> func,
			int retryCount = 10,
			int delayInMs = 2000)
		{
			var result = func();

			int counter = 0;
			while ((result is true) && counter < retryCount)
			{
				Thread.Sleep(delayInMs);
				result = func();
				counter++;
			}

			return result;
		}
	}
}
#endif