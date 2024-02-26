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
	
		[Test]
		public void SwappingEmptyViews()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("FilterItems");
			App.Click("FilterItems");
			App.EnterText("FilterItems", "abcdef");

			// Default empty view
			App.WaitForElement("Nothing to see here.");

			App.Click("ToggleEmptyView");

			// Other empty view
			App.WaitForElement("No results matched your filter.");
		}
	}
}
