using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Bugzilla35733 : IssuesUITest
	{
		public Bugzilla35733(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "iOS WebView crashes when loading an URL with encoded parameters";

		[Test]
		[Category(UITestCategories.WebView)]
		public void Bugzilla35733Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("btnGo");
			RunningApp.Tap("btnGo");
			RunningApp.WaitForElement("WebViewTest");
			RunningApp.Screenshot("I didn't crash");
		}
	}
}