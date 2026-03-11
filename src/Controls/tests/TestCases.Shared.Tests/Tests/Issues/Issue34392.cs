#if ANDROID //WebCiewClient is only used in Android
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
			VerifyInternetConnectivity();
			App.WaitForElement("StatusLabel", timeout: TimeSpan.FromSeconds(15),
				predicate: e => e.GetText() == "SUCCESS");
			var text = App.FindElement("StatusLabel").GetText();
			Assert.That(text, Is.EqualTo("SUCCESS"), $"Expected ShouldOverrideUrlLoading to be called and update the status label text to 'SUCCESS', but got '{text}' instead.");
		}
	}
}
#endif
