using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue7102 : IssuesUITest
	{
		public Issue7102(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CollectionView Header cause delay to adding items."; 
		
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void HeaderDoesNotBreakIndexes()
		{
			RunningApp.WaitForElement("entryInsert");
			RunningApp.Tap("entryInsert");
			RunningApp.ClearText("entryInsert");
			RunningApp.EnterText("entryInsert", "1");
			RunningApp.Tap("Insert");

			// If the bug is still present, then there will be 
			// two "Item: 0" items instead of the newly inserted item
			// Or the header will have disappeared
			RunningApp.WaitForElement("Inserted");
			RunningApp.WaitForElement("This is the header");
		}
	}
}
