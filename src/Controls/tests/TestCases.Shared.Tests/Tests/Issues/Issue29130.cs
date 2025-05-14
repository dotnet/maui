#if TEST_FAILS_ON_WINDOWS //ItemSizingStrategy not working in Windows, Issue:https://github.com/dotnet/maui/issues/29130
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue29130 : _IssuesUITest
	{
		public override string Issue => "CollectionView2 ItemSizingStrategy should work for MeasureFirstItem";
		public Issue29130(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ItemSizeShouldRespondForItemSizingStatergy()
		{
			// Is a iOS issue; see https://github.com/dotnet/maui/issues/29130
			App.WaitForElement("29130Grid");
			var cv = App.WaitForElement("29130CollectionView");
			var cvHeight = cv.GetRect();
			App.WaitForElement("29130MeasureAllItemsButton");
			App.Tap("29130MeasureAllItemsButton");
			var cv2Height = cv.GetRect();
			Assert.That(cv2Height.Height > cvHeight.Height, "CollectionView height should be greater after changing ItemSizingStrategy to MeasureAllItems");
		}
	}
}
#endif
