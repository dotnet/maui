#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue5830 : IssuesUITest
	{
		public Issue5830(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Enhancement] EntryCellTableViewCell should be public";

		[Test]
		[Category(UITestCategories.ListView)]
		[FailsOnIOS]
		public void Issue5830Test()
		{
			RunningApp.WaitForElement("TestReady");
			RunningApp.Screenshot("EntryTableViewCell Test with custom Text and TextColor");
		}
	}
}
#endif