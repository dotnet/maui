using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19786 : _IssuesUITest
	{
		public Issue19786(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[Android] Crash removing item from CarouselView";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void RemovingItemsShouldNotCauseCrash()
		{
			_ = App.WaitForElement("addItemButton");
			App.Click("addItemButton");
			App.Click("addItemButton");
			App.Click("addItemButton");
			App.Click("goToNextItemButton");
			App.Click("goToNextItemButton");
			App.Click("removeLastItemButton");
			App.Click("removeLastItemButton");
			App.Click("removeLastItemButton");
		}

	}
}
