using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    internal class Issue26843 : _IssuesUITest
    {
        public Issue26843(TestDevice device) : base(device) { }

        public override string Issue => "WebView Fails to Load URLs with Certain Encoded Characters on Android";

        [Test]
        [Category(UITestCategories.WebView)]
        [TestCase("https://example.com/test-Ağ-Sistem%20Bilgi%20Güvenliği%20Md/Guide.pdf", "TurkishChars")]
        [TestCase("https://google.com/[]", "SquareBrackets")]
        public async Task WebViewShouldLoadEncodedUrl(string url, string screenshotName)
        {
            App.WaitForElement("WebViewSourceEntry");
            App.ClearText("WebViewSourceEntry");
            App.Tap("WebViewSourceEntry");
            App.EnterText("WebViewSourceEntry", url);
            App.WaitForElement("LoadUrlButton");
            App.Tap("LoadUrlButton");
            await Task.Delay(1000);
            VerifyScreenshot($"{TestContext.CurrentContext.Test.MethodName}_with_{screenshotName}");
        }
    }
}
