﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8262 : _IssuesUITest
	{
		public Issue8262(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] ImageRenderer still being accessed after control destroyed";

		[Fact]
		[Trait("Category", UITestCategories.Image)]
		[Trait("Category", UITestCategories.LifeCycle)]
		[Trait("Category", UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void ScrollingQuicklyOnCollectionViewDoesntCrashOnDestroyedImage()
		{
			App.WaitForElement("ScrollMe");
			App.ScrollDown("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			App.ScrollUp("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			App.ScrollDown("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			App.ScrollUp("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			App.ScrollDown("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			App.WaitForElement("ScrollMe");
		}
	}
}
