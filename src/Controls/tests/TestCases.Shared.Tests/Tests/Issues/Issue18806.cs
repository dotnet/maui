﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18806 : _IssuesUITest
{
	public Issue18806(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "SwipeItemView won't render FontImageSource on first opening";
	
	[Test]
	[Category(UITestCategories.SwipeView)]
	public void ItemImageSourceShouldBeVisible()
	{
		App.WaitForElement("button");
		App.Tap("button");

		VerifyScreenshot();
	}
}