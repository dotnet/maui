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
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void HeaderDoesNotBreakIndexes()
		{
			RunningApp.WaitForElement("entryInsert");
			RunningApp.Tap("entryInsert");
			RunningApp.ClearText("entryInsert");
			RunningApp.EnterText("entryInsert", "1");
			RunningApp.Tap("btnInsert");

			// If the bug is still present, then there will be 
			// two "Item: 0" items instead of the newly inserted item
			// Or the header will have disappeared
			RunningApp.WaitForNoElement("Inserted");
			RunningApp.WaitForNoElement("This is the header");
		}
	}
}
