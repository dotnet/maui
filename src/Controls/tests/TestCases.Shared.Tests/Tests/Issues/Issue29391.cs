#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue29391 : _IssuesUITest
	{
		public Issue29391(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] IsSwipeEnabled Not Working on CarouselView (CV2)";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void IsSwipeEnabledShouldWork()
		{
			App.WaitForElement("Item1");
			App.ScrollRight("CarouselView", swipePercentage: 0.8);
			App.WaitForElement("Item2");
			App.Tap("Switch");
			App.ScrollRight("CarouselView", swipePercentage: 0.8);
			App.WaitForElement("Item2");
		}
	}
}
#endif