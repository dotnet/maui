﻿#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19926 : _IssuesUITest
{
	public override string Issue => "[Android] Opacity bug on BoxView.Background";

	public Issue19926(TestDevice device)
		: base(device)
	{ }

	[Test]
	[Category(UITestCategories.BoxView)]
	public async Task PropertiesShouldBeCorrectlyApplied()
	{
		_ = App.WaitForElement("boxView");
		App.Click("button");
		_ = App.WaitForElement("boxView2");

		// A small delay to wait for the button ripple effect animation to complete.
		await Task.Delay(500);
		VerifyScreenshot();
	}
}
#endif
