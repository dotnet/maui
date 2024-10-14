#if !MACCATALYST
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue17390 : _IssuesUITest
	{
		public override string Issue => "Shell bottom padding when navigating between tabs";

		public Issue17390(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Shell)]
		public void ShellBottomPaddingWhenNavigatingBetweenTabs()
		{
			// Is a iOS issue; see https://github.com/dotnet/maui/issues/17390
			App.WaitForElement("InnerTabbedPageButton");
			App.Tap("InnerTabbedPageButton");
			App.WaitForElement("OpenNonTabbedPage");
			App.Tap("OpenNonTabbedPage");
			App.WaitForElement("BackToTabbedPageButton");
			App.Tap("BackToTabbedPageButton");
			VerifyScreenshot();
		}
	}
}
#endif
