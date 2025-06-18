#if ANDROID || IOS //This test case verifies "IsKeyboardShown method" exclusively on the Android and IOS platforms
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18740 : _IssuesUITest
	{
		public Issue18740(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Virtual keyboard appears with focus on Entry";

		[Test]
		[Category(UITestCategories.Entry)]
		[TestCase("Entry")]
		[TestCase("Editor")]
		[TestCase("SearchBar")]
		public void Issue18740Test(string view)
		{
			try
			{
				// Navigate to the specific View test
				App.WaitForElement("WaitForStubControl");
				App.Tap($"{view}Button");

				// 1.Make sure keyboard starts out closed.	
				App.WaitForElement("WaitForStubControl");
				App.DismissKeyboard();

				// 2. Focus the Entry.
				App.EnterText($"Test{view}", "test");
				App.Tap($"Test{view}");

				// 3. Verify that the virtual keyboard appears.
				Assert.That(App.IsKeyboardShown(), Is.True);
			}
			finally
			{
				this.Back();
			}
		}
	}
}
#endif