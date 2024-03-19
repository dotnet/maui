using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue15357 : _IssuesUITest
	{
		private string buttonId = "ButtonClick";

		public Issue15357(TestDevice device) : base(device)
		{
		}

		public override string Issue => "IsVisible binding not showing items again if Shadow is set";

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

			Assert.AreEqual(GetExpectedButtonStatus(isVisible: true), initialStatus);
			Assert.AreEqual(GetExpectedButtonStatus(isVisible: false), secondStatus);
			Assert.AreEqual(GetExpectedButtonStatus(isVisible: true), thirdStatus);
		}

		string? TapButtonAndGetStatus()
		{
			App.Click(buttonId);

			return GetStatus();
		}

		string? GetStatus() => App.FindElement("LabelStatus").GetText();

		string GetExpectedButtonStatus(bool isVisible)
		{
			var buttonStatus = isVisible ? "is visible" : "is not visible";

			return $"Test Button {buttonStatus}";
		}
	}
}
