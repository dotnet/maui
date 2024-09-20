#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18452 : _IssuesUITest
{
	public override string Issue => "NullReferenceException throws on Windows when setting Cookies on .NET MAUI WebView";

	public Issue18452(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebViewLoadedWithoutException()
	{
		App.WaitForElement("WebViewControl");

		VerifyScreenshot();
	}

}
#endif