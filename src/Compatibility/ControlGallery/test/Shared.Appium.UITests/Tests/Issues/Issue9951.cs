#if ANDROID
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
		[Category(UITestCategories.Switch)]
		public async Task SwitchColorTest()
		{
			RunningApp.WaitForElement(SwitchId);

			RunningApp.Screenshot("Initial switch state");

			RunningApp.Tap(SwitchId);

			//Delay so that the switch toggling is finished
			await Task.Delay(200);

			RunningApp.Screenshot("Toggled switch state");
		}
	}
}
#endif