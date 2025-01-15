using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22417 : _IssuesUITest
	{
		public Issue22417(TestDevice device) : base(device) { }

		public override string Issue => "[Windows] The list does not show newly added items";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void AddItemsToCarouselViewWorks()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("AddItemButton");
			VerifyScreenshot();
		}
	}
}