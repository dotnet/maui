using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue17057 : _IssuesUITest
	{
		public Issue17057(TestDevice testDevice) : base(testDevice)
		{
		}
		public override string Issue => "Shell FlowDirection not updating properly";

		[Fact]
		[Category(UITestCategories.Shell)]
		public void ShellFlowDirectionUpdate()
		{
			App.WaitForElement("label");
			App.TapShellFlyoutIcon();
			VerifyScreenshot();
		}
	}
}