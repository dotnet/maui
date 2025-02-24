using NUnit.Framework;
using UITest.Appium;
using UITest.Appium.AI;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue5669 : _IssuesUITest
	{
		public override string Issue => "Windows SearchBar MaxLength > 0 not working properly";
		public Issue5669(TestDevice testDevice) : base(testDevice)
		{
		}

		[Test]
		[Category(UITestCategories.SearchBar)]
		public void SearchBarMaxLength()
		{
			App.WaitForElement("SearchBar");
			App.EnterText("SearchBar", "r");
			App.Click("ChangeValue");
			App.EnterText("SearchBar", "r");
			App.EnterText("SearchBar", "c");
#if MACCATALYST
			// On MacCatalyst, pressing the ESC key during screenshot capture clears the text.
			// This causes the image generated in CI to differ from local runs.
			Assert.That(App.WaitForElement("Sear").GetText(), Is.EqualTo("Sear"));
#else
			VerifyScreenshot();
#endif
		}

		[Test]
		[Category(UITestCategories.SearchBar)]
		[Ignore("The necessary Azure OpenAI parameters are still to be added on CI")]
		public async Task SearchBarMaxLengthWithAI()
		{
			await App.ExecuteWithAI(@"
			This test verify that the MaxLength property of the SearchBar control is working correctly.
			1. Tap on the SearchBar control to set the focus.
			2. Type the letter r in the SearchBar.
			3. Type the letter r in the SearchBar.
			4. Tap the Button to change the MaxLength property value.
			5. Type the letter c in the SearchBar.");

			Assert.That(App.WaitForElement("Sear").GetText(), Is.EqualTo("Sear"));
		}
	}
}