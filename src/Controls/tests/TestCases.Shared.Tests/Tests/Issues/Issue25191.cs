#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25191 : _IssuesUITest
	{
		public Issue25191(TestDevice device) : base(device)
		{
		}

		public override string Issue => "CollectionView ItemSizingStrategy:MeasureFirstItem renders labels incorrectly";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewMeasureFirstItem()
		{
			App.WaitForElement("collectionView");
			VerifyScreenshot();
		}
	}
}
#endif