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
		public void Issue3884Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("TestReady");
			App.Screenshot("I see a blue circle");
		}
	}
}
