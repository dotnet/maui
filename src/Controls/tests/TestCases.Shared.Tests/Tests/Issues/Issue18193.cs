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
			App.WaitForElementTillPageNavigationSettled("NavigationToPage6Button");
			App.Tap("NavigationToPage6Button");
			App.WaitForElementTillPageNavigationSettled("NavigateToDetailButton");
			App.Tap("NavigateToDetailButton");
			App.WaitForElementTillPageNavigationSettled("NavigateBackButton");
			App.Tap("NavigateBackButton");
			App.WaitForElementTillPageNavigationSettled("NavigateToPage2Button");
			App.Tap("NavigateToPage2Button");
			App.WaitForElementTillPageNavigationSettled("NavigateToPage5Button");
			App.Tap("NavigateToPage5Button");
			App.WaitForElementTillPageNavigationSettled("More");
			App.Tap("More");
		}
	}
}