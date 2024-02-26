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
		public void HeaderDoesNotBreakIndexes()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("entryInsert");
			App.Click("entryInsert");
			App.ClearText("entryInsert");
			App.EnterText("entryInsert", "1");
			App.Click("Insert");

			// If the bug is still present, then there will be 
			// two "Item: 0" items instead of the newly inserted item
			// Or the header will have disappeared
			App.WaitForElement("Inserted");
			App.WaitForElement("This is the header");
		}
	}
}
