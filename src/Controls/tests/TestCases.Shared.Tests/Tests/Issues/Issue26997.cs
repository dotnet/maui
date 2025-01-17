using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26997 : _IssuesUITest
	{
		public override string Issue => "CollectionView should not crash on iOS 15 and 16";

		public Issue26997(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewShouldNotCrash()
		{
			App.WaitForElement("collectionView");
		}
	}
}
