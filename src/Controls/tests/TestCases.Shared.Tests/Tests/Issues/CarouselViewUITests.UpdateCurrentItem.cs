#if TEST_FAILS_ON_WINDOWS //For more information : https://github.com/dotnet/maui/issues/24482
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

		public override string Issue => "CarouselView does not update the CurrentItem on Swipe under strange condition";

		// Issue9827 (src\ControlGallery\src\Issues.Shared\Issue9827.cs
		[Test]
		[Category(UITestCategories.CarouselView)]
		public void Issue9827Test()
		{
			App.WaitForElement("Pos:0");
			App.WaitForElement("btnNext");
			App.Click("btnNext");
			App.WaitForElement("Item 1 with some additional text");
			App.WaitForElement("Pos:1");
		}
	}
}
#endif