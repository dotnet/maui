using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21980 : _IssuesUITest
	{
		public Issue21980(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[Android] IndicatorView with DataTemplate (custom image) does not render correctly when ItemsSource change.";

		[Test]
		[Category(UITestCategories.IndicatorView)]
		[FailsOnMac("VerifyScreenshot method not implemented on macOS")]
		[FailsOnIOS("Need to create new issue for iOS platform")]
		[FailsOnWindows("Need to create new issue for windows platform")]
		public async Task ShouldIndicatorViewUpdateProperlyWhenChangeIndicatorViewItemsSource()
		{
			App.WaitForElement("IndicatorView");

			// 1.The test fails if the placeholder text in the editor below is missing.
			App.Tap("BtnChangeSource");

			// Delay for the Editor underline on Android to return from
			// the selected state to normal state.
			await Task.Delay(500);

			// 2. The test fails if the placeholder text in the editor below is not blue.
			VerifyScreenshot();
		}
	}
}