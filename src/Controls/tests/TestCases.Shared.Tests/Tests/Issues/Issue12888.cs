using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12888 : _IssuesUITest
	{
		public Issue12888(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Fix timing on iOS Toolbar";

		[Category(UITestCategories.Shell)]
		[Test(Description = "TitleView Set On Shell Works After Navigation")]
		public void TitleViewSetOnShellWorksAfterNavigation()
		{
			App.WaitForElement("Success");

			App.Click("GoToItem2");
			App.WaitForElement("Success");

			App.Click("GoToItem1");
			App.WaitForElement("Success");

			App.Click("GoToItem2");
			App.WaitForElement("Success");

			App.Click("PushPage");
			App.WaitForElement("Success");

			App.Click("PopPage");
			App.WaitForElement("Success");
		}
	}
}