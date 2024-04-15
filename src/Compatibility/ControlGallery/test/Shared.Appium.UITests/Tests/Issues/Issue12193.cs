using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue12193 : IssuesUITest
	{
		const string HTML = "HTML";

		public Issue12193(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CarouselView content disappears after 2 rotations if TextType=Html is used";

		[Test]
		[Category(UITestCategories.CarouselView)]
		[FailsOnIOS]
		public async Task RotatingCarouselViewHTMLShouldNotDisappear()
		{
			int delay = 3000;

			RunningApp.SetOrientationPortrait();
			await Task.Delay(delay);

			RunningApp.SetOrientationLandscape();
			await Task.Delay(delay);

			RunningApp.SetOrientationPortrait();
			await Task.Delay(delay);

			RunningApp.SetOrientationLandscape();
			await Task.Delay(delay);

			RunningApp.SetOrientationPortrait();
			await Task.Delay(delay);

			RunningApp.WaitForElement(HTML);
		}
	}
}