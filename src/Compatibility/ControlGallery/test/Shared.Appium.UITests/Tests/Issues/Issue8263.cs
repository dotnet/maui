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
		public void SwitchOnOffVisualStatesTest()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("Switch");
			App.Screenshot("Switch Default");
			App.Click("Switch");
			App.Screenshot("Switch Off with Red ThumbColor");
			App.Click("Switch");
			App.Screenshot("Switch On with Green ThumbColor");
		}
	}
}
