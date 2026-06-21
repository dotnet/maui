using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue29615 : _IssuesUITest
	{
		public Issue29615(TestDevice testDevice) : base(testDevice) { }

		public override string Issue => "Flyout icon is displayed when flyout is disabled on iOS and MacCatalyst";

		[Test]
		[Category(UITestCategories.Shell)]
		public void VerifyFlyoutIconIsNotPresentWhenDisabledFlyout()
		{
			App.WaitForElement("DisabledButton");
			App.WaitForFlyoutIcon();
			App.Tap("DisabledButton");
			App.WaitForNoFlyoutIcon();
		}
	}
}