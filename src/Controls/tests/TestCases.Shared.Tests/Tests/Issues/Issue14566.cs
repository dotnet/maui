using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue14566 : _IssuesUITest
	{
		public Issue14566(TestDevice testDevice) : base(testDevice)
		{
		}

		const string SearchBar = "SearchBar";
		const string ResultText = "ResultText";
		const string CheckResultButton = "CheckResultButton";
		const string Success = "Success";
		const string Failure = "Failure";
		public override string Issue => "SearchBar IsEnabled property not functioning";

		[Test]
		[Category(UITestCategories.SearchBar)]
		public void SearchBarShouldRespectIsEnabled()
		{
			App.WaitForElement(CheckResultButton);
			App.Tap(SearchBar);
			App.EnterText(SearchBar, "Hello");
			App.Tap(CheckResultButton);
			var resultText = App.WaitForElement(ResultText).GetText();
			Assert.That(resultText, Is.EqualTo(Success));


		}
	}
}