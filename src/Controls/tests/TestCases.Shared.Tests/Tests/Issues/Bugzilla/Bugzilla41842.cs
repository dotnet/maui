using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla41842 : _IssuesUITest
	{
		public Bugzilla41842(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Set FlyoutPage.Detail = New Page() twice will crash the application when set FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split";

		[Test]
		[Category(UITestCategories.FlyoutPage)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla41842Test()
		{
			App.WaitForElement("SuccessLabel");
		}
	}
}