using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue8263 : IssuesUITest
	{
		public Issue8263(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Enhancement] Add On/Off VisualStates for Switch"; 
		
		[Test]
		[Category(UITestCategories.Switch)]
		[FailsOnIOS]
		public void SwitchOnOffVisualStatesTest()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("Switch");
			RunningApp.Screenshot("Switch Default");
			RunningApp.Tap("Switch");
			RunningApp.Screenshot("Switch Off with Red ThumbColor");
			RunningApp.Tap("Switch");
			RunningApp.Screenshot("Switch On with Green ThumbColor");
		}
	}
}
