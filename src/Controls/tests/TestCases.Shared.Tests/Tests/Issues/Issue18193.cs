#if TEST_FAILS_ON_CATALYST //related issue: https://github.com/dotnet/maui/issues/27206
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
			App.WaitForElementTillPageNavigationSettled("Navigate to page 6");
			App.Tap("Navigate to page 6");
			App.WaitForElementTillPageNavigationSettled("Navigate to detail page");
			App.Tap("Navigate to detail page");
			App.WaitForElementTillPageNavigationSettled("Navigate back");
			App.Tap("Navigate back");
			App.WaitForElementTillPageNavigationSettled("Navigate to Page 2");
			App.Tap("Navigate to Page 2");
			App.WaitForElementTillPageNavigationSettled("Navigate to page 5");
			App.Tap("Navigate to page 5");
			App.WaitForElementTillPageNavigationSettled("More");
			App.Tap("More");
		}
	}
}
#endif