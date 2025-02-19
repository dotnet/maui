using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25502 : _IssuesUITest
	{
		public Issue25502(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Gray Line Appears on the Right Side of GraphicsView with Decimal WidthRequest on iOS Platform";

		[FlakyTest("Issue to reenable this test: https://github.com/dotnet/maui/issues/27798")]
		[Category(UITestCategories.GraphicsView)]
		public void VerifyGraphicsViewWithoutGrayLine()
		{
			App.WaitForElement("ChangeColorButton");
			App.Click("ChangeColorButton");
			VerifyScreenshot();
		}
	}
}