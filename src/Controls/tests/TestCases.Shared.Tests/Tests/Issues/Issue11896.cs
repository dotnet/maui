#if TEST_FAILS_ON_WINDOWS //for more information:https://github.com/dotnet/maui/issues/24968
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue11896 : _IssuesUITest
	{
		public Issue11896(TestDevice device) : base(device) { }

		public override string Issue => "CollectionView Header/Footer/EmptyView issues when adding/removing items";

		[Fact]

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

		[Fact]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewHeaderBlankWhenLastItemRemoved()
		{
			App.WaitForElement("AddButton");
			App.Tap("RemoveButton");
			App.Tap("RemoveButton");
			App.Tap("RemoveButton");
			App.Tap("AddButton");
			// Here we check for Header and Footer proper visibility with proper alignment in view.
			VerifyScreenshot();
		}
	}
}
#endif