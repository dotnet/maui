using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue10274 : _IssuesUITest
	{
		public Issue10274(TestDevice device) : base(device)
		{
		}

		public override string Issue => "MAUI Flyout does not work on Android when not using Shell";

		[Fact]
		[Category(UITestCategories.FlyoutPage)]
		public void FlyoutPageNavigation()
		{
			App.WaitForElement("mainPageButton");
			App.Tap("mainPageButton"); // Navigate to FlyoutPage using PushAsync()

			var flyoutLabel = App.WaitForElement("flyoutPageLabel");
			Assert.Equal("This is FlyoutPage", flyoutLabel.GetText());

			App.Tap("flyoutPageButton"); // Navigate to MainPage using PopAsync()

			var mainLabel = App.WaitForElement("mainPageLabel");
			Assert.Equal("This is MainPage", mainLabel.GetText());
		}
	}
}