#if ANDROID || IOS
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
			App.WaitForElement("changeItemsSource");
			await Task.Delay(500);
			App.Click("changeItemsSource");
			await Task.Delay(500);
			VerifyScreenshot();
		}
	}
}
#endif