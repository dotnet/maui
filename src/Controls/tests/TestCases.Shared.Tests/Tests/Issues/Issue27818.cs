using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27818 : _IssuesUITest
	{
		public Issue27818(TestDevice device) : base(device) { }

		public override string Issue => "CarouselView crashes on iOS 15.8.3 when using CarouselViewHandler2";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void CarouselViewShouldNotCrash()
		{
			App.WaitForElement("CarouselView");
		}
	}
}