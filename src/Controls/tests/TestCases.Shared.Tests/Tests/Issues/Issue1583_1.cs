#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1583_1 : _IssuesUITest
	{
		public Issue1583_1(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "WebView fails to load from urlwebviewsource with non-ascii characters (works with Uri)";

		[Test]
		[Category(UITestCategories.WebView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		public async Task Issue1583_1_WebviewTest()
		{
			App.WaitForElement("label", "Could not find label", TimeSpan.FromSeconds(10), null, null);
			await Task.Delay(TimeSpan.FromSeconds(3));
			App.Screenshot("I didn't crash and i can see Skøyen");
			App.Tap("hashButton");
			await Task.Delay(TimeSpan.FromSeconds(3));
			App.Screenshot("I didn't crash and i can see the GitHub comment #issuecomment-389443737");
			App.Tap("queryButton");
			await Task.Delay(TimeSpan.FromSeconds(3));
			App.Screenshot("I didn't crash and i can see google search for http://microsoft.com");
		}
	}
}
#endif