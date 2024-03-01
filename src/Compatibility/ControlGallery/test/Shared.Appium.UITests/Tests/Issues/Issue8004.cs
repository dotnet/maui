using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests.Tests.Issues
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
		public async Task AnimateScaleOfBoxView()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Windows]);

			App.WaitForElement("TestReady");
			App.Screenshot("Small blue box");

			// Check the box and button elements.
			App.WaitForElement(BoxToScale);
			App.WaitForElement(AnimateBoxViewButton);

			// Tap the button.
			App.Click(AnimateBoxViewButton);

			// Wait for animation to finish.
			await Task.Delay(500);

			App.Screenshot("Bigger blue box");
		}
	}
}