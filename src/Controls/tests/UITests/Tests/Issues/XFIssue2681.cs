using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class XFIssue2681 : _IssuesUITest
	{
		const string NavigateToPage = "ClickMe";

		public XFIssue2681(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[UWP] Label inside Listview gets stuck inside infinite loop";

		[Test]
		[Category(UITestCategories.ListView)]
		public void ListViewDoesntFreezeApp()
		{
			App.WaitForElement(NavigateToPage);
			App.Click(NavigateToPage);
			App.WaitForElement("3");
			App.Click("GoBack");
		}
	}
}
