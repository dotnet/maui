using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23502 : _IssuesUITest
	{
		public override string Issue => "WebView Navigated event is not triggered";

		public Issue23502(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.WebView)]
		public void VerifyWebViewNavigatedEvent()
		{
			VerifyInternetConnectivity();
			var navigatingLabel = App.WaitForElement("navigatingLabel");
			var navigatedLabel = App.WaitForElement("navigatedLabel");

			Assert.That(navigatingLabel.GetText(), Is.EqualTo("Navigating event is triggered"));
			Assert.That(navigatedLabel.GetText(), Is.EqualTo("Navigated event is triggered"));
		}
	}
}
