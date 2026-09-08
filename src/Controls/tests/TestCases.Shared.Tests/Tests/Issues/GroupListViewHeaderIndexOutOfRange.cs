using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class GroupListViewHeaderIndexOutOfRange : _IssuesUITest
	{
		const string ButtonId = "button";
		public GroupListViewHeaderIndexOutOfRange(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Group ListView Crashes when ItemSource is Cleared";

		[Test]
		[Category(UITestCategories.ListView)]
		public void GroupListViewHeaderIndexOutOfRangeTest()
		{
			App.WaitForElement(ButtonId);
			App.Tap(ButtonId);
			App.WaitForElement(ButtonId);
		}
	}
}