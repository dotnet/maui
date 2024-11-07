#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25518 : _IssuesUITest
	{
		public Issue25518(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[Windows] Fix for the screen does not display when changing the CurrentPage of a TabbedPage";

		[Test]
		[Category(UITestCategories.TabbedPage)]
		public void VerifyTabbedPageContent()
		{
			App.WaitForElement("MoveToSecondPage");
			App.Tap("MoveToSecondPage");

			VerifyScreenshot();
		}
	}
}
#endif
