using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25371 : _IssuesUITest
	{
		public override string Issue => "OnNavigatedTo not called when navigating back to a specific page";

		public Issue25371(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		public void ValidateOnNavigationToMethod()
		{
			App.WaitForElement("MainPageLabel");
			App.Tap("MoveToNextPage");
			App.WaitForElement("SecondPageLabel");

			App.LongPress("Back");
			var navigationToLabel = App.FindElement("MainPageLabel").GetText();
			if (string.IsNullOrEmpty(navigationToLabel))
			{
				Assert.Fail("OnNavigationTo method is not called");
			}
			else
			{
				Assert.That(navigationToLabel, Is.EqualTo("OnNavigationTo method is called"));
			}
		}
	}
}
