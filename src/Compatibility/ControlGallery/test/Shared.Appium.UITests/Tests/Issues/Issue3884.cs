using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue3884 : IssuesUITest
	{
		public Issue3884(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "BoxView corner radius";
		
		[Test]
		[Category(UITestCategories.BoxView)]
		[FailsOnIOS]
		public void Issue3884Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("TestReady");
			RunningApp.Screenshot("I see a blue circle");
		}
	}
}
