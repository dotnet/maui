using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21814 : _IssuesUITest
	{
		public override string Issue => "Add better parameters for navigation args";

		public Issue21814(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		public void VerifyNavigationEventArgs()
		{
			App.WaitForElement("NavigateButton");

			var navigatedToBeforeNavigate = App.FindElement("OnNavigatedToLabel").GetText();
			Assert.That(navigatedToBeforeNavigate, Is.EqualTo("PreviousPage: Null, NavigationType: Push"));
			
			var navigatingFromBeforeNavigate = App.FindElement("OnNavigatingFromLabel").GetText();
			Assert.That(navigatingFromBeforeNavigate, Is.EqualTo(string.Empty));

			var navigatedFromBeforeNavigate = App.FindElement("OnNavigatedFromLabel").GetText();
			Assert.That(navigatedFromBeforeNavigate, Is.EqualTo(string.Empty));

			App.Tap("NavigateButton");

			var navigatedToPage2 = App.FindElement("OnNavigatedToLabel").GetText();
			Assert.That(navigatedToPage2, Is.EqualTo("PreviousPage: Issue21814FirstPage, NavigationType: Push"));

			var navigatingFromPage2 = App.FindElement("OnNavigatingFromLabel").GetText();
			Assert.That(navigatingFromPage2, Is.EqualTo(string.Empty));

			var navigatedFromPage2 = App.FindElement("OnNavigatedFromLabel").GetText();
			Assert.That(navigatedFromPage2, Is.EqualTo(string.Empty));

			App.Tap("NavigateBackButton");

			var navigatedToAfterNavigate = App.FindElement("OnNavigatedToLabel").GetText();
			Assert.That(navigatedToAfterNavigate, Is.EqualTo("PreviousPage: Issue21814SecondPage, NavigationType: Pop"));

			var navigatingFromAfterNavigate = App.FindElement("OnNavigatingFromLabel").GetText();
			Assert.That(navigatingFromAfterNavigate, Is.EqualTo("DestinationPage: Issue21814SecondPage, NavigationType: Push"));

			var navigatedFromAfterNavigate = App.FindElement("OnNavigatedFromLabel").GetText();
			Assert.That(navigatedFromAfterNavigate, Is.EqualTo("DestinationPage: Issue21814SecondPage, NavigationType: Push"));

		}
	}
}