using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Bugzilla34007 : IssuesUITest
	{
		public Bugzilla34007(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Z order drawing of children views are different on Android, iOS, Win";

		[Test]
		[Category(UITestCategories.Layout)]
		public void Issue34007TestFirstElementHasLowestZOrder()
		{
			var buttonLocations = RunningApp.WaitForElement("Button0");

			var rect = buttonLocations.GetRect();
			var x = rect.CenterX();
			var y = rect.CenterY();

			// Button 1 was the last item added to the grid; it should be tappable
			RunningApp.Tap("Button1");

			// The label should indicate that Button 1 was the last button tapped
			RunningApp.WaitForNoElement("Button 1 was tapped last");

			RunningApp.Screenshot("Buttons Reordered");

			// Tapping Button1 1 reordered the buttons in the grid; Button 0 should
			// now be on top. Tapping at the Button 1 location should actually tap
			// Button 0, and the label should indicate that
			RunningApp.TapCoordinates(x, y);
			RunningApp.WaitForNoElement("Button 0 was tapped last");

			RunningApp.Screenshot("Button 0 Tapped");
		}
	}
}