using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
#if ANDROID
using OpenQA.Selenium;
#endif

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

		public override string Issue => "SearchBar IsEnabled property not functioning";

		[Test]
		[Category(UITestCategories.SearchBar)]
		public void SearchBarShouldRespectIsEnabled()
		{
			App.WaitForElement(CheckResultButton);
			App.Tap(SearchBar);
#if ANDROID
			try
			{
				App.EnterText(SearchBar, "Hello");
			}
			catch(InvalidElementStateException)
			{
				Assert.Pass("SearchBar is disabled");
			}
#else
			App.EnterText(SearchBar, "Hello");
#endif
			App.Tap(CheckResultButton);
			var resultText = App.WaitForElement(ResultText).GetText();
			Assert.That(resultText, Is.EqualTo("Success"));

		}
	}
}