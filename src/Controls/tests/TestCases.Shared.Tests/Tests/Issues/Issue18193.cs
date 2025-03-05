using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18193 : _IssuesUITest
	{
		public override string Issue => "[iOS] Navigation doesn't work on sixth tab in shell";

		public Issue18193(TestDevice testDevice) : base(testDevice)
		{
		}

		[Test]
		[Category(UITestCategories.Shell)]
		public void ShellNavigationShouldWorkInMoreTab()
		{
			App.WaitForElementTillPageNavigationSettled("NavigationToPageSixthButton");
			App.Tap("NavigationToPageSixthButton");
			App.WaitForElementTillPageNavigationSettled("NavigateToDetailButton");
			App.Tap("NavigateToDetailButton");
			App.WaitForElementTillPageNavigationSettled("NavigateBackButton");
			App.Tap("NavigateBackButton");
			App.WaitForElementTillPageNavigationSettled("NavigateToPageTwoButton");
			App.Tap("NavigateToPageTwoButton");
			App.WaitForElementTillPageNavigationSettled("NavigateToPageFiveButton");
			App.Tap("NavigateToPageFiveButton");
			App.WaitForElementTillPageNavigationSettled("More");
			App.Tap("More");
		}
	}
}