using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25181 : _IssuesUITest
	{
		public override string Issue => "CollectionView item alignment issue in HorizontalGrid layout when only one item is present";

		public Issue25181(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void SingleItemAlignmentInCollectionViewHorizontalGridLayout()
		{
			App.WaitForElement("collectionview");

			VerifyScreenshot();
		}
	}
}
