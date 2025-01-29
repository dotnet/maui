using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue9951 : _IssuesUITest
	{
		const string SwitchId = "switch";

		public Issue9951(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Android 10 Setting ThumbColor on Switch causes a square block";

		[Test]
		[Category(UITestCategories.Switch)]
		[Category(UITestCategories.Compatibility)]
		public async Task SwitchColorTest()
		{
			App.WaitForElement(SwitchId);

			VerifyScreenshot("SwitchColorTest_before_toggling");

			App.Tap(SwitchId);

			//Delay so that the switch toggling is finished
			await Task.Delay(200);

			VerifyScreenshot("SwitchColorTest_after_toggling");
		}
	}
}