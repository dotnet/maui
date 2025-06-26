﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2858 : _IssuesUITest
	{
		const string Success = "Success";
		const string InnerGrid = "InnerGrid";
		const string OuterGrid = "OuterGrid";

		public Issue2858(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Transparency Cascading";

		[Fact]
		[Trait("Category", UITestCategories.Layout)]
		[Trait("Category", UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void CascadeInputTransparentGrids()
		{
			App.WaitForElement(InnerGrid);
			App.Tap(InnerGrid);

			var green = App.WaitForElement(OuterGrid).GetRect();
			App.TapCoordinates(green.CenterX(), green.Y + 20);
			App.WaitForElement(Success);
		}
	}
}