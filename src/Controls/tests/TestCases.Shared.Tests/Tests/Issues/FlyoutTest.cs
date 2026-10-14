using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class FlyoutTest : _IssuesUITest
	{
		public override string Issue => "FlyoutTest";

		public FlyoutTest(TestDevice device)
		: base(device)
		{ }

		[Test]
		[ShardedTestCategory(UITestCategories.Shell, shard: 1), Order(1)]
		public void VerifyHamburgerIcon()
		{
			App.WaitForElement("Tab1Page");
			VerifyScreenshot();
		}

		[Test]
		[ShardedTestCategory(UITestCategories.Shell, shard: 1), Order(2)]
		public void VerifyFlyoutBackgroundColor()
		{
			App.WaitForElement("Tab1Page");
			App.Tap("ChangeFlyoutBackground");
			App.TapShellFlyoutIcon();
			VerifyScreenshot();
		}
	}
}