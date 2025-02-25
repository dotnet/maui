using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1219 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue1219(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Setting ToolbarItems in ContentPage constructor crashes app";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		public void ViewCellInTableViewDoesNotCrash()
		{
			App.WaitForElement(Success);
		}
	}
}