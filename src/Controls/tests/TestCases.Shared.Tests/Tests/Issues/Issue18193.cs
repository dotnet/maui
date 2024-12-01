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
			App.Click("NavigateToDetailButton");
			App.Click("NavigateBackButton");
			App.Click("NavigateToPage2Button");
			App.WaitForElement("Page2Label");
		}
	}
}
#endif