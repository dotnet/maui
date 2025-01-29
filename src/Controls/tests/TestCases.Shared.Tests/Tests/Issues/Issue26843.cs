#if TEST_FAILS_ON_WINDOWS //for more information:https://github.com/dotnet/maui/issues/24968
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
 
namespace Microsoft.Maui.TestCases.Tests.Issues
{
    internal class Issue26843 : _IssuesUITest
    {
        public Issue26843(TestDevice device) : base(device) { }
 
        public override string Issue => "WebView Fails to Load URLs with Certain Encoded Characters on Android";
        
        /// <summary>
		/// This test validates that absolute URIs are not treated as relative ones. 
		/// Notably, we test URIs with non-Western characters (e.g., "Ğ" and spaces encoded as "%20").
		/// </summary>
        [Test]
        [Category(UITestCategories.WebView)]
        [TestCase("https://example.com/test-Ağ-Sistem%20Bilgi%20Güvenliği%20Md/Guide.pdf")]
        [TestCase("https://google.com/[]")]
        public void WebViewShouldLoadEncodedUrl(string url)
        {
            App.WaitForElement("WebViewSourceEntry");
            App.ClearText("WebViewSourceEntry");
            App.Tap("WebViewSourceEntry");
            App.EnterText("WebViewSourceEntry", url);
            App.WaitForElement("LoadUrlButton");
            App.Tap("LoadUrlButton");
            var label = App.WaitForElement("NavigationResultLabel");
            Assert.That(label.GetText(), Is.EqualTo("Successfully navigated to the encoded URL"));
        }
 
        public override void TestSetup()
        {
            base.TestSetup();
 
            try
            {
                App.WaitForElement("NoInternetAccessLabel", timeout: TimeSpan.FromSeconds(1));
                Assert.Inconclusive("This device doesn't have internet access");
            }
            catch (TimeoutException)
            {
                // Element not found within timeout, assume internet is available
                // Continue with the test
            }
        }
    }
}
#endif