using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue15253 : _IssuesUITest
	{
		public override string Issue => "[Windows]HorizontalScrollBarVisibility doesnot works";
		public Issue15253(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void HorizontalScrollBarShouldHideOnNever()
		{
			// Is a Windows issue; see https://github.com/dotnet/maui/issues/15253
			App.WaitForElement("15253CarouselView");
			App.Tap("one");
			VerifyScreenshot();
		}
	}
}
