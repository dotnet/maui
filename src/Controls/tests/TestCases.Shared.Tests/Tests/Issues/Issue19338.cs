using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19338 : _IssuesUITest
	{
		public Issue19338(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Border.Shadow hide the collectionView Header";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void HeaderAndFooterShouldBeVisible()
		{
			App.WaitForElement("HeaderLabel");
			VerifyScreenshot();
		}
	}
}