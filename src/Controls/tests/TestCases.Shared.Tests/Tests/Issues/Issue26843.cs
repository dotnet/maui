using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

	internal class Issue26843 : _IssuesUITest
	{
		public Issue26843(TestDevice device) : base(device) { }

		public override string Issue => "WebView Fails to Load URLs with Certain Encoded Characters on Android";

		[Test]
		[Category(UITestCategories.WebView)]
		public void WebViewShouldLoadEncodedUrl()
		{
			var label = App.WaitForElement("NavigationResultLabel");
			App.WaitForElement("WebView");
			Assert.That(label.ReadText(), Is.EqualTo("Successfully navigated to the encoded URL"));
		}
	}

