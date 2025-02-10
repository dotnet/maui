using NUnit.Framework;
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

		[Test]
		[Category(UITestCategories.FlyoutPage)]
		public void FlyoutPageNavigation()
		{
			App.WaitForElement("mainPageButton");
			App.Tap("mainPageButton"); // Navigate to FlyoutPage using PushAsync()

			var flyoutLabel = App.WaitForElement("flyoutPageLabel");
			Assert.That(flyoutLabel.GetText(), Is.EqualTo("This is FlyoutPage"));

			App.Tap("flyoutPageButton"); // Navigate to MainPage using PopAsync()

			var mainLabel = App.WaitForElement("mainPageLabel");
			Assert.That(mainLabel.GetText(), Is.EqualTo("This is MainPage"));
		}
	}
}