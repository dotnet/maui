using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22899 : _IssuesUITest
	{
		public override string Issue => "Title not updated after OnAppearing for TabbedPage in NavigationPage";

		public Issue22899(TestDevice device) : base(device) { }

		[Fact]
		[Trait("Category", UITestCategories.TabbedPage)]
		public void Issue22899Test()
		{
			_ = App.WaitForElement("button");
			App.Click("button");
			_ = App.WaitForElement("label");

			// The test passes if the text in the navigation bar has changed
			VerifyScreenshot();
		}
	}
}
