using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25074_2 : _IssuesUITest
	{
		public Issue25074_2(TestDevice testDevice) : base(testDevice) { }

		public override string Issue => "Button title can extend past previously truncated size";

		[Fact]
		[Trait("Category", UITestCategories.Button)]
		public void ButtonTitleFillsSpaceWhenImageChanges()
		{
			App.WaitForElement("Button1");
			VerifyScreenshot(GetCurrentTestName() + "Original");
			App.Tap("Button1");
			VerifyScreenshot(GetCurrentTestName() + "Altered");
			App.Tap("Button1");
			VerifyScreenshot(GetCurrentTestName() + "Original");
		}
	}
}