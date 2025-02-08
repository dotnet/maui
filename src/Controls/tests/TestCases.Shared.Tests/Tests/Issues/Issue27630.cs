#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27630 : _IssuesUITest
	{
		public Issue27630(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Hidden ListView doesn't appear as expected";

		[Test]
		[Category(UITestCategories.ListView)]
		public void ListViewShouldBeVisible()
		{
			App.WaitForElement("Button");
			App.Click("Button");
			App.WaitForElement("ViewCellLabel");
		}
	}
}
#endif