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
		public void SwitchColorTest()
		{
			App.WaitForElement(SwitchId);

			VerifyScreenshot("SwitchColorTest_before_toggling");

			App.Tap(SwitchId);

			VerifyScreenshot("SwitchColorTest_after_toggling");
		}
	}
}