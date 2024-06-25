using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla57674 : _IssuesUITest
	{
		public Bugzilla57674(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ListView not honoring INotifyCollectionChanged";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid]
		[FailsOnIOS]
		[FailsOnMac]
		public void Bugzilla57674Test()
		{
			App.Screenshot("Initial Status");
			App.WaitForElement("IssueListView");
			App.Tap("IssueButton");
			App.Screenshot("Element Added to List");
		}
	}
}