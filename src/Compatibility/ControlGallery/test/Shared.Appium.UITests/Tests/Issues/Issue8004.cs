using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue8004 : IssuesUITest
	{
		const string AnimateBoxViewButton = "AnimateBoxViewButton";
		const string BoxToScale = "BoxToScale";

		public Issue8004(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Add a ScaleXTo and ScaleYTo animation extension method";

		[Test]
		[Category(UITestCategories.Animation)]
		public async Task AnimateScaleOfBoxView()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Windows]);

			RunningApp.WaitForElement("TestReady");
			RunningApp.Screenshot("Small blue box");

			// Check the box and button elements.
			RunningApp.WaitForElement(BoxToScale);
			RunningApp.WaitForElement(AnimateBoxViewButton);

			// Tap the button.
			RunningApp.Tap(AnimateBoxViewButton);

			// Wait for animation to finish.
			await Task.Delay(500);

			RunningApp.Screenshot("Bigger blue box");
		}
	}
}