using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue22417 : _IssuesUITest
	{
		public Issue22417(TestDevice device) : base(device) { }

		public override string Issue => "[MAUI] The list does not show newly added recipes";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public async Task AddItemToCarousel()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.iOS, TestDevice.Mac });

			App.WaitForElement("WaitHere");

			App.WaitForElement("AddButton");

			await Task.Delay(500);

			VerifyScreenshot();
		}
	}
}