using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue22032 : _IssuesUITest
	{
		public Issue22032(TestDevice device) : base(device) { }

		public override string Issue => "Shell FlyoutItem Tab Selected Icon Color not changing if using Font icons";

		[Test]
		[Category(UITestCategories.ScrollView)]
		public async Task SelectedTabIconShouldChangeColor()
		{
			App.WaitForElement("button");

			App.Click("button");

			await Task.Delay(500);

			// The test passes if tab1 icon is green and tab2 red
			VerifyScreenshot();
		}
	}
}
