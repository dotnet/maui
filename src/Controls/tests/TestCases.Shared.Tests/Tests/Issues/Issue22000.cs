using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22000 : _IssuesUITest
	{
		public Issue22000(TestDevice device) : base(device) { }

		public override string Issue => "[Windows] Carousel changes the current position when window is resized";

		[Fact]
		[Trait("Category", UITestCategories.CarouselView)]
		public async Task ResizeCarouselViewKeepsIndex()
		{
			App.WaitForElement("WaitForStubControl");

			for (int i = 0; i < 10; i++)
			{
				App.Click("UpdateSizeButton");
			}

			await Task.Delay(500);

			VerifyScreenshot();
		}
	}
}