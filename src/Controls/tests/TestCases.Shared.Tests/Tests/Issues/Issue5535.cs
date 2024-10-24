using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue5535 : _IssuesUITest
	{
		public Issue5535(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView: Swapping EmptyViews has no effect";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void SwappingEmptyViews()
		{
			App.WaitForElement("FilterItems");
			App.Tap("FilterItems");
			App.EnterText("FilterItems", "abcdef");

			// Default empty view
			App.WaitForNoElement("Nothing to see here.");

			App.Tap("ToggleEmptyView");

			// Other empty view
			App.WaitForNoElement("No results matched your filter.");
		}
	}
}