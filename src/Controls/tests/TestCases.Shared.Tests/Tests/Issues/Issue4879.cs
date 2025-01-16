using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue4879 : _IssuesUITest
	{
		public Issue4879(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "4879 - ImageButtonPadding";

		[Test]
		[Category(UITestCategories.ImageButton)]
		[Category(UITestCategories.Compatibility)]
		public void Issue4879Test()
		{
			App.WaitForElement("TestReady");
			VerifyScreenshot();
		}
	}
}
