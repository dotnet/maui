using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

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
		[Category(UITestCategories.ListView)]
		public async Task WhenTapButtonThenListViewsChangesVisibility()
		{
			App.WaitForElement(buttonId);

			var initialStatus = GetStatus();
			var secondStatus = TapButtonAndGetStatus();
			await Task.Delay(500);
			var thirdStatus = TapButtonAndGetStatus();
			await Task.Delay(500);

			Assert.AreEqual(GetExpectedListsStatus("List 1"), initialStatus);
			Assert.AreEqual(GetExpectedListsStatus("List 2"), secondStatus);
			Assert.AreEqual(GetExpectedListsStatus("List 1"), thirdStatus);
		}

		string? TapButtonAndGetStatus()
		{
			App.Click(buttonId);

			return GetStatus();
		}

		string? GetStatus() => App.FindElement("LabelStatus").GetText();

		string GetExpectedListsStatus(string listName) => $"{listName} is now visible";
	}
}
