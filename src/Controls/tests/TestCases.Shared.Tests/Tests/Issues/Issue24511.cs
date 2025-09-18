using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24511 : _IssuesUITest
	{
		public Issue24511(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Spacing in the ItemsLayout of CollectionView stops it from scrolling";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewWithSpacingShouldScroll()
		{
			App.WaitForElement("CV");
			App.ScrollDown("CV");

			VerifyScreenshot();
		}
	}
}