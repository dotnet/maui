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
		[FailsOnAndroidWhenRunningOnXamarinUITest]
		public void WebViewDisposesProperly()
		{
			App.Tap("NextButton");
			App.Tap("BackButton");
			App.Tap("NextButton");
			App.Tap("BackButton");
			App.Tap("NextButton");
			App.Tap("BackButton");
		}
	}
}