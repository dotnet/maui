#if !MACCATALYST && !WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue11896 : _IssuesUITest
	{
		public Issue11896(TestDevice device) : base(device) { }

		public override string Issue => "CollectionView Header/Footer/EmptyView issues when adding/removing items";

		[Test]

		[Category(UITestCategories.CollectionView)]
		public void CollectionviewFooterHideswhenDynamicallyAddorRemoveItems()
		{
			App.WaitForElement("AddButton");
			App.Tap("AddButton");
			App.Tap("AddButton");
			App.Tap("AddButton");
			// Here we check for Footer proper visibility with proper alignment in view.
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewHeaderBlankAndFooterShiftsWhenLastItemRemoved()
		{
			App.WaitForElement("AddButton");
			App.Tap("AddButton");
			App.Tap("AddButton");
			App.Tap("RemoveButton");
			App.Tap("RemoveButton");
			App.Tap("AddButton");
			// Here we check for Header and Footer proper visibility with proper alignment in view.
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewFooterVisibilityWhenDynamicEmptyViewOnItemRemoval()
		{
			App.WaitForElement("AddButton");
			App.Tap("AddButton");
			App.Tap("AddButton");
			App.Tap("RemoveButton");
			App.Tap("RemoveButton");
			// Here we check for dynamic Emptyview and Footer proper proper alignment in view it should not be at the bottom of screen.
			VerifyScreenshot();
		}
	}
}
#endif