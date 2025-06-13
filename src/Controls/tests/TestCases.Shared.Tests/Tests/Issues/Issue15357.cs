using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue15357 : _IssuesUITest
	{
		private string buttonId = "ButtonClick";

		public Issue15357(TestDevice device) : base(device)
		{
		}

		public override string Issue => "IsVisible binding not showing items again if Shadow is set";

		[Fact]
		[Category(UITestCategories.ListView)]
		public async Task WhenTapButtonThenListViewsChangesVisibility()
		{
			App.WaitForElement(buttonId);

			var initialStatus = GetStatus();
			var secondStatus = TapButtonAndGetStatus();
			await Task.Delay(500);
			var thirdStatus = TapButtonAndGetStatus();
			await Task.Delay(500);

			Assert.Equal(GetExpectedButtonStatus(isVisible: true), initialStatus);
			Assert.Equal(GetExpectedButtonStatus(isVisible: false), secondStatus);
			Assert.Equal(GetExpectedButtonStatus(isVisible: true), thirdStatus);
		}

		string? TapButtonAndGetStatus()
		{
			App.Tap(buttonId);

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
