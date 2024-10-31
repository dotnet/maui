using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24105 : _IssuesUITest
	{
		public override string Issue => "CarouselView doesn't scroll to the right Position after changing the ItemSource with Loop enabled";

		public Issue24105(TestDevice testDevice) : base(testDevice) { }
		
		[Test]
		[Category(UITestCategories.CarouselView)]
		public void CarouselViewShouldScrollToRightPosition()
		{
			_ = App.WaitForElement("goToLastItemButton");
			App.Click("goToLastItemButton");
			App.WaitForElement("five");
			App.Click("reloadItemsButton");
			App.WaitForElement("five2");
		}
	}
}