#if ANDROID || IOS //Issue is only reproducible on Android and iOS platforms.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27715 : _IssuesUITest
	{

		public Issue27715(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ScrollView measures incorrectly when it has AdjustedContentInsets";

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void ScrollViewShouldRenderWithinBounds()
		{
			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}
#endif