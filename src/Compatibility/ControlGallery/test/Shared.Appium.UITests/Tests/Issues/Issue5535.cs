using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue5535 : IssuesUITest
	{
		public Issue5535(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView: Swapping EmptyViews has no effect";
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void SwappingEmptyViews()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("FilterItems");
			RunningApp.Tap("FilterItems");
			RunningApp.EnterText("FilterItems", "abcdef");

			// Default empty view
			RunningApp.WaitForElement("Nothing to see here.");

			RunningApp.Tap("ToggleEmptyView");

			// Other empty view
			RunningApp.WaitForElement("No results matched your filter.");
		}
	}
}
