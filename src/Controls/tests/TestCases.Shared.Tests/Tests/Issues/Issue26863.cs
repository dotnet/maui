using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26863 : _IssuesUITest
	{
		public Issue26863(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CarouselView with CarouselViewHandler2 make app crash";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void AppShouldNotCrashOnScrollingCarouselViewWithoutLoop()
		{
			App.WaitForElement("button");
			App.Click("button");

			//The test passes if no crash is observed
		}
	}
}