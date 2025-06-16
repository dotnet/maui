using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24061 : _IssuesUITest
	{
		public Issue24061(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ScrollView's gradient background doesn't work with ScrollView";

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void ScrollViewGradientBackgroundShouldWorkWithScrollView()
		{
			App.WaitForElement("Item2");
			App.SwipeRightToLeft("Item2", 0.99);

			VerifyScreenshot("ScrollViewGradientBackground");
			App.Click("button");
			VerifyScreenshot("ScrollViewWithoutGradientBackground");
			App.Click("button");
			VerifyScreenshot("ScrollViewGradientBackground");
		}
	}
}