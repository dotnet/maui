using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue11962 : IssuesUITest
	{
		public Issue11962(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Cannot access a disposed object. Object name: 'WkWebViewRenderer";

		[Test]
		[Category(UITestCategories.WebView)]
		[FailsOnAndroid]
		public void WebViewDisposesProperly()
		{
			RunningApp.Tap("NextButton");
			RunningApp.Tap("BackButton");
			RunningApp.Tap("NextButton");
			RunningApp.Tap("BackButton");
			RunningApp.Tap("NextButton");
			RunningApp.Tap("BackButton");
		}
	}
}