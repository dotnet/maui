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
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatforms]
		public async Task AnimateScaleOfBoxView()
		{
			App.WaitForElement("TestReady");
			App.Screenshot("Small blue box");

			// Check the box and button elements.
			App.WaitForElement(BoxToScale);
			App.WaitForElement(AnimateBoxViewButton);

			// Tap the button.
			App.Tap(AnimateBoxViewButton);

			// Wait for animation to finish.
			await Task.Delay(500);

			App.Screenshot("Bigger blue box");
		}
	}
}