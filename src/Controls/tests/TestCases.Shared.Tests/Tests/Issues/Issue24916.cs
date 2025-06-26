using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24916 : _IssuesUITest
	{
		public Issue24916(TestDevice testDevice) : base(testDevice) { }

		public override string Issue => "Image Source in Button initializes correctly";

		[Fact]
		[Trait("Category", UITestCategories.Button)]
		public void ImageSourceInitializesCorrectly()
		{
			App.WaitForElement("label");
			VerifyScreenshot();
		}
	}
}
