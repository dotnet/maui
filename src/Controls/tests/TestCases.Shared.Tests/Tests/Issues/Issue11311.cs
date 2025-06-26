using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue11311 : _IssuesUITest
	{
		public Issue11311(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Regression] CollectionView NSRangeException";

		[Fact]
		[Trait("Category", UITestCategories.CollectionView)]
		[Trait("Category", UITestCategories.TabbedPage)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void CollectionViewWithFooterShouldNotNSRangeExceptionCrashOnDisplay()
		{
			// If this hasn't already crashed, the test is passing
			App.FindElement("Success");
		}
	}
}