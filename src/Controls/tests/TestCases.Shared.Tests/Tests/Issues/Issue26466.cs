using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26466 : _IssuesUITest
	{
		const string ButtonId = "thebutton";
		const string SuccessId = "success";

		public Issue26466(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Button is not released on unload";

		[Test]
		[Category(UITestCategories.Button)]
		public async Task ButtonReleasedTest()
		{
			App.WaitForElement(ButtonId);
			App.Tap(ButtonId);
			await Task.Delay(200);

			// Button should be unloaded, so we should see the success label
			App.WaitForElement(SuccessId);
		}
	}
}