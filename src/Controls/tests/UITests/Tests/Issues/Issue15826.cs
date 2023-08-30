using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue15826 : _IssuesUITest
	{
		private string buttonId = "Swap";

		public Issue15826(TestDevice device) : base(device)
		{
		}

		public override string Issue => "ListView visibility doesn't work well";

		[Test]
		public void WhenTapButtonThenListViewsChangesVisibility()
		{
			App.WaitForElement(buttonId);

			var initialStatus = GetStatus();
			var secondStatus = TapButtonAndGetStatus();
			var thirdStatus = TapButtonAndGetStatus();

			Assert.AreEqual(GetExpectedListsStatus("List 1"), initialStatus);
			Assert.AreEqual(GetExpectedListsStatus("List 2"), secondStatus);
			Assert.AreEqual(GetExpectedListsStatus("List 1"), thirdStatus);
		}

		string TapButtonAndGetStatus()
		{
			App.Tap(buttonId);

			return GetStatus();
		}

		string GetStatus() => App.Query(x => x.Marked("LabelStatus"))[0].Text;

		string GetExpectedListsStatus(string listName) => $"{listName} is now visible";
	}
}
