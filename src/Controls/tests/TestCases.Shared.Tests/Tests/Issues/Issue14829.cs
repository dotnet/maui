using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue14829 : _IssuesUITest
	{
		public Issue14829(TestDevice device) : base(device)
		{
		}

		public override string Issue => "DisplayActionSheet still not working on Windows";

		[Test]
		[Category(UITestCategories.ActionSheet)]
		public void DisplayActionSheetStillNotWorkingOnWindows()
		{
			App.WaitForElement("DisplayActionSheetButton", timeout: TimeSpan.FromSeconds(4)).Click();
			App.WaitForElement("ActionSheet: Send to?", timeout: TimeSpan.FromSeconds(4));
			App.WaitForElement("Email");
		}
	}
}