using NUnit.Framework;
using UITest.Appium;

namespace UITests 
{ 
    internal class Issue2653 : IssuesUITest
	{
		const string ButtonText = "Insert Box View";
		const string TestForButtonClicked = "Test For Clicked";
		const string Success = "BoxView Not Overlapping";

		public Issue2653(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[UWP] Grid insert z-order on UWP broken in Forms 3";

		[Test]
		public void ZIndexWhenInsertingChildren()
		{
			App.WaitForNoElement(ButtonText);
			App.Click(ButtonText);
			App.Click(ButtonText);
			App.Click(TestForButtonClicked);
			App.WaitForNoElement(Success);
		}
	}
}