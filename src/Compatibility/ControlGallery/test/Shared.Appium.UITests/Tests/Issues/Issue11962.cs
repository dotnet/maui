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
		public void WebViewDisposesProperly()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Tap("NextButton");
			RunningApp.Tap("BackButton");
			RunningApp.Tap("NextButton");
			RunningApp.Tap("BackButton");
			RunningApp.Tap("NextButton");
			RunningApp.Tap("BackButton");
		}
	}
}