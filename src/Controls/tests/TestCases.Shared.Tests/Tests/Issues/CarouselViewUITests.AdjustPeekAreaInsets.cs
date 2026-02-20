using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CarouselViewAdjustPeekAreaInsets : _IssuesUITest
	{
		public CarouselViewAdjustPeekAreaInsets(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "[Bug] Java.Lang.IllegalArgumentException in CarouselView adjusting PeekAreaInsets in OnSizeAllocated using XF 5.0";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void ChangePeekAreaInsetsInOnSizeAllocatedTest()
		{
			// Use longer timeout for CarouselView which can be slow to render on CI
			App.WaitForElement("CarouselId", timeout: TimeSpan.FromSeconds(30));
		}
	}
}