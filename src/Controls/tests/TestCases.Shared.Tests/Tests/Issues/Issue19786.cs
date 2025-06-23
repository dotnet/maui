using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
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
			App.Tap("addItemButton");
			App.WaitForElement("addItemButton");
			App.Tap("addItemButton");
			App.WaitForElement("addItemButton");
			App.Tap("addItemButton");
			App.WaitForElement("goToNextItemButton");
			App.Tap("goToNextItemButton");
			App.WaitForElement("goToNextItemButton");
			App.Tap("goToNextItemButton");
			App.WaitForElement("removeLastItemButton");
			App.Tap("removeLastItemButton");
			App.WaitForElement("removeLastItemButton");
			App.Tap("removeLastItemButton");
			App.WaitForElement("removeLastItemButton");
			App.Tap("removeLastItemButton");
		}
	}
}
