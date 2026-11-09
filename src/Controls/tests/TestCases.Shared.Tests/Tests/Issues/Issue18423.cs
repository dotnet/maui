using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18423 : _IssuesUITest
	{
		public Issue18423(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Windows] Shell flyout template items do not have a margin applied on first show";

		[Test]
		[ShardedTestCategory(UITestCategories.Shell, shard: 2)]
		public void VerifyTemplateViewMarginOnInitialDisplay()
		{
			App.WaitForElement("MainPageLabel");
			App.TapShellFlyoutIcon();
			VerifyScreenshot();
		}
	}
}