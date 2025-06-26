﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Trait("Category", UITestCategories.Border)]
	public class Issue22606 : _IssuesUITest
	{
		public Issue22606(TestDevice device) : base(device) { }

		public override string Issue => "Border does not expand on Content size changed";

		[Fact]
		public void BorderBackgroundExpandsOnContentSizeChanged()
		{
			App.WaitForElement("SetHeightTo200");
			App.Tap("SetHeightTo200");
			VerifyScreenshot("Issue22606_SetHeightTo200");

			App.Tap("SetHeightTo500");
			VerifyScreenshot("Issue22606_SetHeightTo500");
		}

#if ANDROID || IOS  //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.
		[Fact]
		public void BorderBackgroundSizeUpdatesWhenRotatingScreen()
		{
			App.WaitForElement("SetHeightTo200");
			App.Tap("SetHeightTo200");
			App.SetOrientationLandscape();
			VerifyScreenshot();
		}
#endif
	}
}