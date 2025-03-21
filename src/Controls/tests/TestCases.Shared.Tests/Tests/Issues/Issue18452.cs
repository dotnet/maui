using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.WebView)]
public class Issue18452 : _IssuesUITest
{
	public override string Issue => "NullReferenceException throws on Windows when setting Cookies on .NET MAUI WebView";
	string? expected = "Success";

	public Issue18452(TestDevice device) : base(device)
	{
	}

	[Test]
	public void WebViewLoadedWithoutException()
	{
		VerifyInternetConnectivity();
		App.WaitForElement("Label");
		string? label = App.FindElement("Label").GetText();
		Assert.That(label, Is.EqualTo(expected));
	}
}
