using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1583 : IssuesUITest
	{
		public Issue1583(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NavigationPage.TitleIcon broken"; 
		
		[Test]
		[Category(UITestCategories.Navigation)]
		[FailsOnIOS]
		public void Issue1583TitleIconTest()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("lblHello");
		}
	}
}