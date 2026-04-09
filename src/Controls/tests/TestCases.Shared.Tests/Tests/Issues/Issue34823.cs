using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34823 : _IssuesUITest
{
	public Issue34823(TestDevice device) : base(device)
	{
	}

	public override string Issue => "WebView on Windows Does Not Inherit App Theme";

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebViewWithExplicitBackgroundKeepsLightScheme_WhenAppThemeIsDark()
	{
		App.WaitForElement("ThemedWebView");
		App.Tap("ForceDarkThemeButton");
		App.Tap("ReadColorSchemeButton");

		var resultText = App.FindElement("ResultLabel").GetText();
		Assert.That(resultText, Does.Contain("Scheme=light"));
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebViewColorSchemeProbe_Simplified()
	{
		App.WaitForElement("ThemedWebView");
		App.Tap("ForceLightThemeButton");
		App.Tap("ReadColorSchemeButton");

		var resultText = App.FindElement("ResultLabel").GetText();
		Assert.That(resultText, Does.StartWith("Scheme="));
		Assert.That(resultText, Does.Not.Contain("Error="));
	}
}