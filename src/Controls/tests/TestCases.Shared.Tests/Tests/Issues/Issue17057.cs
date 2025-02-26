#if TEST_FAILS_ON_WINDOWS // To fix the issue in windows https://github.com/dotnet/maui/issues/27947
using NUnit.Framework;
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

		[Test]
		[Category(UITestCategories.Shell)]
		public void ShellFlowDirectionUpdate()
		{
			App.WaitForElement("label");
			VerifyScreenshot();
		}
	}
}
#endif