#if IOS
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
			App.WaitForElement("NavigationToPage6Button");
			App.Click("NavigationToPage6Button");
			App.WaitForElement("NavigateToDetailButton");
			App.Click("NavigateToDetailButton");
			App.WaitForElement("NavigateBackButton");
			App.Click("NavigateBackButton");
			App.WaitForElement("NavigateToPage2Button");
			App.Click("NavigateToPage2Button");
			App.WaitForElement("NavigateToPage5Button");
			App.Click("NavigateToPage5Button");
			App.WaitForElement("More");
			App.Click("More");
		}
	}
}
#endif