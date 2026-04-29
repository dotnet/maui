#if ANDROID //WebViewClient is only used in Android
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue34392 : _IssuesUITest
	{
		public Issue34392(TestDevice testDevice) : base(testDevice)
		{
		}
		public override string Issue => "MAUI Handler not working with Custom WebView on Android (ShouldOverrideUrlLoading behavior)";

		[Test]
		[Category(UITestCategories.WebView)]
		public void ShouldOverrideUrlLoading_Called()
		{
			// The label starts as "FAILED" and updates asynchronously after ShouldOverrideUrlLoading fires.
    // Poll until the text changes to "SUCCESS" instead of reading a stale initial value.
    var result = App.WaitForTextToBePresentInElement("StatusLabel", "SUCCESS", timeout: TimeSpan.FromSeconds(10));
    Assert.That(result, Is.True, "Expected ShouldOverrideUrlLoading to be called and update the status label text to 'SUCCESS', but it was not updated within the timeout.");
		}
	}
}
#endif
