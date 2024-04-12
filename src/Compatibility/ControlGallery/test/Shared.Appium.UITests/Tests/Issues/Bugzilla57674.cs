using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Bugzilla57674 : IssuesUITest
	{
		public Bugzilla57674(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ListView not honoring INotifyCollectionChanged ";

		[Test]
		[Category(UITestCategories.ListView)]
		[FailsOnIOS]
		public void Bugzilla57674Test()
		{
			RunningApp.Screenshot("Initial Status");
			RunningApp.WaitForElement("IssueListView");
			RunningApp.Tap("IssueButton");
			RunningApp.Screenshot("Element Added to List");
		}
	}
}