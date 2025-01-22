﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14825 : _IssuesUITest
{
	public override string Issue => "Capture WebView screenshot";

	public Issue14825(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public async Task ValidateWebViewScreenshot()
	{
		VerifyInternetConnectivity();
		App.WaitForElement("TestInstructions");
		await Task.Delay(TimeSpan.FromSeconds(5));
		// Click the capture button to capture a WebView screenshot.
		App.Click("Capture");
		VerifyScreenshot();
	}
	 
}
