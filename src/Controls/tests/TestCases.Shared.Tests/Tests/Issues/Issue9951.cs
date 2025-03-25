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
		public void SwitchColorTestBeforeToggling()
		{
			App.WaitForElement(SwitchId);

			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Switch)]
		public void SwitchColorTestAfterToggling()
		{
			App.WaitForElement(SwitchId);
			App.Tap(SwitchId);
			App.WaitForElement(SwitchId);

			VerifyScreenshot();

		}
	}
}