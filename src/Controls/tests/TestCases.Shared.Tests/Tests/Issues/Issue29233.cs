using System;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues;

public class Issue29233 : _IssuesUITest
{
	public Issue29233(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Android WebView Navigated is fired without setting source";

	[Fact]
	[Trait("Category", UITestCategories.WebView)]
	public void NavigatedShouldNotFireWithNullSource()
	{
		App.WaitForElement("WaitLabel", timeout: TimeSpan.FromSeconds(5));
		App.WaitForNoElement("MauiLabel");
	}
}
