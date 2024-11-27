﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla34007 : _IssuesUITest
	{
		public Bugzilla34007(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Z order drawing of children views are different on Android, iOS, Win";

		[Test]
		[Category(UITestCategories.Layout)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		public void Issue34007TestFirstElementHasLowestZOrder()
		{
			var buttonLocations = App.WaitForElement("Button0");

			var rect = buttonLocations.GetRect();
			var x = rect.CenterX();
			var y = rect.CenterY();

			// Button 1 was the last item added to the grid; it should be tappable
			App.Tap("Button1");

			// The label should indicate that Button 1 was the last button tapped
			App.WaitForNoElement("Button 1 was tapped last");

			App.Screenshot("Buttons Reordered");

			// Tapping Button1 1 reordered the buttons in the grid; Button 0 should
			// now be on top. Tapping at the Button 1 location should actually tap
			// Button 0, and the label should indicate that
			App.TapCoordinates(x, y);
			App.WaitForNoElement("Button 0 was tapped last");

			App.Screenshot("Button 0 Tapped");
		}
	}
}