using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21706 : _IssuesUITest
	{
		public override string Issue => "ImageButton stuck in PointerOver state";

		public Issue21706(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.ImageButton)]
		public async Task ImageButtonStuckAfterRightClick()
		{
			App.WaitForElement("WaitForElement");
			App.RightClick("WaitForElement");
			await Task.Delay(200);
			App.Click("OtherButton");
			await Task.Delay(200);
			VerifyScreenshot();
		}
	}
}