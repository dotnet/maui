using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue5669 : _IssuesUITest
	{
		public override string Issue => "Windows SearchBar MaxLength > 0 not working properly";
		public Issue5669(TestDevice testDevice) : base(testDevice)
		{
		}

		[Fact]
		[Trait("Category", UITestCategories.SearchBar)]
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
	}
}