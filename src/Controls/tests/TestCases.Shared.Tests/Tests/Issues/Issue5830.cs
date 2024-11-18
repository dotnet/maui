#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue5830 : _IssuesUITest
	{
		public Issue5830(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Enhancement] EntryCellTableViewCell should be public";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		public void Issue5830Test()
		{
			App.WaitForElement("TestReady");
			App.Screenshot("EntryTableViewCell Test with custom Text and TextColor");
		}
	}
}
#endif