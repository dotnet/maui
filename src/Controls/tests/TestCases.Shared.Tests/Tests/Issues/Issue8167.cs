using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8167 : _IssuesUITest
	{
		const string Run = "Update Text";
		const string Success = "Success";

		public Issue8167(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] XF 4.3 UWP Crash - Element not found";

		[Test]
		[Category(UITestCategories.Compatibility)]
		public void ThreadpoolBindingUpdateShouldNotCrash()
		{
			App.WaitForElement(Run);
			App.Tap(Run);
			App.WaitForElement(Success);
		}
	}
}
