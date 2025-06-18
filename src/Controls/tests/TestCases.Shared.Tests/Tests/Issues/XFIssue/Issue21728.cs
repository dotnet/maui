using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21728 : _IssuesUITest
	{
		public override string Issue => "CollectionView item alignment issue when a single item is present with a footer";

		public Issue21728(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewSingleItemAlignmentWithFooter()
		{
			App.WaitForElement("collectionview");

			VerifyScreenshot();
		}
	}
}
