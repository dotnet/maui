using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8814 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue8814(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] UWP Shell cannot host CollectionView/CarouselView";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewInShellShouldBeVisible()
		{
			App.WaitForElement(Success);
		}
	}
}