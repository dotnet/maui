using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3884 : _IssuesUITest
	{
		public Issue3884(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "BoxView corner radius";

		[Fact]
		[Trait("Category", UITestCategories.BoxView)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void Issue3884Test()
		{
			App.WaitForElement("You should see a blue circle");
			VerifyScreenshot();
		}
	}
}