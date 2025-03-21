using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8004 : _IssuesUITest
	{
		const string AnimateBoxViewButton = "AnimateBoxViewButton";
		const string BoxToScale = "BoxToScale";

		public Issue8004(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Add a ScaleXTo and ScaleYTo animation extension method";

		[Test]
		[Category(UITestCategories.Animation)]
		public void AnimateScaleOfBoxView()
		{
			App.WaitForElement("TestReady");
			//The BoxView's AutomationId doesn't work correctly on the Windows platform, and using a Label also doesn't ensure the BoxView's size changes.
			//Issue Link: https://github.com/dotnet/maui/issues/27195
#if WINDOWS
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_SmallBox");
#else
			var rect = App.WaitForElement(BoxToScale).GetRect();
#endif
			App.WaitForElement(AnimateBoxViewButton);

			// Tap the button.
			App.Tap(AnimateBoxViewButton);

			// Wait for animation to finish.
			Thread.Sleep(500);
#if WINDOWS
			VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_BigBox");
#else
			var scaledRect = App.WaitForElement(BoxToScale).GetRect();
			Assert.That(scaledRect.Width, Is.GreaterThan(rect.Width));
			Assert.That(scaledRect.Height, Is.GreaterThan(rect.Height));
#endif
		}
	}
}