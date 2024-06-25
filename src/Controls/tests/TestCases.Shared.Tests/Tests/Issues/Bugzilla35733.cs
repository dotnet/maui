using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla35733 : _IssuesUITest
	{
		public Bugzilla35733(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "iOS WebView crashes when loading an URL with encoded parameters";

		[Test]
		[Category(UITestCategories.WebView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatforms]
		public void Bugzilla35733Test()
		{
			App.WaitForElement("btnGo");
			App.Tap("btnGo");
			App.WaitForElement("WebViewTest");
			App.Screenshot("I didn't crash");
		}
	}
}