using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1590 : IssuesUITest
	{
		public Issue1590(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ListView.IsGroupingEnabled results ins ArguementOutOfRangeException"; 
		
		[Test]
		[Category(UITestCategories.ListView)]
		[FailsOnIOS]
		public void ListViewIsGroupingEnabledDoesNotCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForNoElement("First");
		}
	}
}