using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue10234 : IssuesUITest
	{
		public Issue10234(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CarouselView disposed on iOS when navigating back in Shell ";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void AppDoesntCrashWhenResettingPage()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("goToShow");
			RunningApp.Tap("goToShow");
			RunningApp.WaitForElement("goToBack");
			ScrollNextItem();
			RunningApp.Tap("goToBack");
			RunningApp.WaitForElement("goToShow");
			RunningApp.Tap("goToShow");
			ScrollNextItem();
			RunningApp.WaitForElement("goToBack");
			RunningApp.Tap("goToBack");
			RunningApp.WaitForElement("goToShow");
		}

		void ScrollNextItem()
		{
			var rect = RunningApp.FindElement("carouselView").GetRect();
			var centerX = rect.CenterX();
			var rightX = rect.X - 5;
			RunningApp.DragCoordinates(centerX + 40, rect.CenterY(), rightX, rect.CenterY());
		}
	}
}