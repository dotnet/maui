using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22306_1 : _IssuesUITest
	{
		public Issue22306_1(TestDevice device) : base(device) { }

		public override string Issue => "Button with images measures correctly";

		[Test]
		[Category(UITestCategories.Button)]
		public async Task ButtonMeasuresLargerThanDefault()
		{
			// seems possible in CI that the iOS Button is not arranged before the height evaluation
			await Task.Delay(100);
			var rotateButtonRect = App.WaitForElement("RotateButton").GetRect();
			var button3Rect = App.WaitForElement("Button3").GetRect();

			Assert.That(rotateButtonRect.Height > button3Rect.Height, $"rotateButtonRect.Height is {rotateButtonRect.Height} is not greater than button3Rect.Height {button3Rect.Height}");
		}

		[Test]
		[Category(UITestCategories.Button)]
		public void ButtonLayoutResizesWithImagePosition()
		{
			App.WaitForElement("RotateButton");
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Top");

			App.WaitForElement("RotateButton").Tap();
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Right");

			App.WaitForElement("RotateButton").Tap();
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Bottom");

			App.WaitForElement("RotateButton").Tap();
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Left");

			App.WaitForElement("RotateButton").Tap();
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "Top");
		}
	}
}
