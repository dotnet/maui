using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25859 : _IssuesUITest
	{
		public override string Issue => "Item spacing not properly applied between items in CollectionView Horizontal LinearItemsLayout";

		public Issue25859(TestDevice device) : base(device)
		{ }

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CVHorizontalLinearItemsLayoutItemSpacing()
		{
			App.WaitForElement("collectionView");

			VerifyScreenshot();
		}
	}
}