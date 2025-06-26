using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1267 : _IssuesUITest
	{
		const string Success = "If this is visible, the test has passed.";

		public Issue1267(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Star '*' in Grid layout throws exception";

		[Fact]
		[Trait("Category", UITestCategories.Layout)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void StarInGridDoesNotCrash()
		{
			App.WaitForElement(Success);
		}
	}
}