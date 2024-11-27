﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue11132 : _IssuesUITest
	{
		const string InstructionsId = "instructions";

		public Issue11132(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] [iOS] UpdateClip throws NullReferenceException when the Name of the Mask of the Layer is null";

		/*
		[Test]
		[Description("Verify that can use a CustomRenderer overriding the iOS View Layer properties")]
		[Category(UITestCategories.Frame)]
		[Category(UITestCategories.CustomHandlers)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		public void Issue11132CustomRendererLayerAndClip()
		{
			App.WaitForElement(InstructionsId);
			App.Screenshot("No crash");
		}
		*/
	}
}