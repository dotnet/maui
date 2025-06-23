using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue11962 : _IssuesUITest
	{
		public Issue11962(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Cannot access a disposed object. Object name: 'WkWebViewRenderer";

		[Test]
		[Category(UITestCategories.WebView)]
		[Category(UITestCategories.Compatibility)]
		public void WebViewDisposesProperly()
		{
			VerifyInternetConnectivity();
			App.WaitForElement("NextButton");
			App.Tap("NextButton");
			App.WaitForElement("BackButton");
			App.Tap("BackButton");
			App.WaitForElement("NextButton");
			App.Tap("NextButton");
			App.WaitForElement("BackButton");
			App.Tap("BackButton");
			App.WaitForElement("NextButton");
			App.Tap("NextButton");
			App.WaitForElement("BackButton");
			App.Tap("BackButton");
		}
	}
}