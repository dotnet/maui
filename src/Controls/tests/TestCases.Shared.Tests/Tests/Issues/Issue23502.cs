using Xunit;
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

		[Fact]
		[Category(UITestCategories.WebView)]
		public void VerifyWebViewNavigatedEvent()
		{
			VerifyInternetConnectivity();
			var navigatingLabel = App.WaitForElement("navigatingLabel");
			var navigatedLabel = App.WaitForElement("navigatedLabel");

			Assert.Equal("Navigating event is triggered", navigatingLabel.GetText());
			Assert.Equal("Navigated event is triggered", navigatedLabel.GetText());
		}
	}
}
