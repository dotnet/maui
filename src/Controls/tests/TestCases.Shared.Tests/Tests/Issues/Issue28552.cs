﻿#if ANDROID
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue28552 : _IssuesUITest
{
	public Issue28552(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Changing StatusBar and NavigationBar background color doesn't work with Modal pages";

	[Fact]
	[Trait("Category", UITestCategories.Navigation)]
	public void StatusBarAndNavigationBarShouldInheritColor()
	{
		App.WaitForElement("OpenModalButton");
		App.Click("OpenModalButton");
		App.WaitForElement("LabelOnModalPage");
		VerifyScreenshot();
	}
}
#endif