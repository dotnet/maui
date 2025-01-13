using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue9419 : _IssuesUITest
	{
		const string OkResult = "Ok";

		public Issue9419(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Crash when toolbar item removed then page changed";
		[Test]
		[Category(UITestCategories.ToolbarItem)]
		[Category(UITestCategories.Compatibility)]
		public void TestIssue9419()
		{
			App.WaitForElementTillPageNavigationSettled(OkResult);
		}
	}
}
