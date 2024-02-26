using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
    public class Issue9951 : IssuesUITest
	{
		const string SwitchId = "switch";

		public Issue9951(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Android 10 Setting ThumbColor on Switch causes a square block"; 
		
		[Test]
		public async Task SwitchColorTest()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement(SwitchId);

			App.Screenshot("Initial switch state");

			App.Click(SwitchId);

			//Delay so that the switch toggling is finished
			await Task.Delay(200);

			App.Screenshot("Toggled switch state");
		}
	}
}