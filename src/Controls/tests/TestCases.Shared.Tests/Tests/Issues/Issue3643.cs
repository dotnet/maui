#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3643 : _IssuesUITest
	{
		public Issue3643(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView items don't change their size when item content size is changed";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ItemCellsResize()
		{
			App.WaitForElement("clickMe");
			VerifyScreenshot("ItemCellsResize_SmallerCellSizes");
			App.Tap("clickMe");
			VerifyScreenshot();
		}
	}
}
#endif