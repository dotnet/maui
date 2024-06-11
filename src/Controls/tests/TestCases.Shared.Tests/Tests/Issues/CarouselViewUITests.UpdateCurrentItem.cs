#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CarouselViewUpdateCurrentItem : _IssuesUITest
	{
		public CarouselViewUpdateCurrentItem(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "CarouselView doesn't update the CurrentItem on Swipe under strange condition";

		// Issue9827 (src\ControlGallery\src\Issues.Shared\Issue9827.cs
		[Test]
		[Category(UITestCategories.CarouselView)]
		public void Issue9827Test()
		{
			App.WaitForNoElement("Pos:0");
			App.Click("btnNext");
			App.WaitForNoElement("Item 1 with some additional text");
			App.WaitForNoElement("Pos:1");
		}
	}
}
#endif