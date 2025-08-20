#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/31239

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19568 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue19568(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "GraphicsView does not change the Background/BackgroundColor on Android";

		[Test]
		[Category(UITestCategories.GraphicsView)]
		public void GraphicsViewShouldHaveBackgroundColor()
		{
			App.WaitForElement("ChangeBackgroundColorButton");
			App.Click("ChangeBackgroundColorButton");
			VerifyScreenshot();
		}
	}
}
#endif