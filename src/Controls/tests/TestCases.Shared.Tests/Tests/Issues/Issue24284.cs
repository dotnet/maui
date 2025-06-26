using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24284 : _IssuesUITest
	{
		public Issue24284(TestDevice testDevice) : base(testDevice) { }

		public override string Issue => "FlyoutHeaderAdaptsToMinimumHeight";

		[Fact]
		[Trait("Category", UITestCategories.Shell)]
		public void FlyoutHeaderAdaptsToMinimumHeight()
		{
			var heightReferenceLabel = App.WaitForElement("HeightReferenceLabel").GetRect();
			var headerLabel = App.WaitForElement("HeaderLabel").GetRect();

			Assert.True(Math.Abs(headerLabel.Height - heightReferenceLabel.Height) < 0.2);
		}
	}
}