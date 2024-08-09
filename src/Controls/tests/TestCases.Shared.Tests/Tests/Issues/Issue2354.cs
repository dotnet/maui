using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2354 : _IssuesUITest
	{
		public Issue2354(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ListView, ImageCell and disabled source cache and same image url";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid]
		[FailsOnMac]
		[FailsOnWindows]
		public void TestDoesntCrashWithCachingDisable()
		{
			App.WaitForElement("ImageLoaded");
			App.ScrollDown("TestListView", ScrollStrategy.Programmatically);
			App.ScrollDown("TestListView", ScrollStrategy.Programmatically);
		}
	}
}