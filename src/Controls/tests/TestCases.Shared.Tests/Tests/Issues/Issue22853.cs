using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue22853 : _IssuesUITest
	{
		public Issue22853(TestDevice device) : base(device)
		{
		}

		public override string Issue => "WordWrap LineBreakMode leaves extra space on Android";

		[Fact]
		[Category(UITestCategories.Label)]
		public void WordWrapLineBreakModeNoExtraSpace()
		{
			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}