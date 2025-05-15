#if !WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26990 : _IssuesUITest
	{
		public Issue26990(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Accessibility only gets set on WrapperView";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void WrapperViewsDoNotBlockTapGestureRecognizerAccessibility()
		{
			App.WaitForElement("RevealButton").Click();
			VerifyScreenshot();
        }
	}
}
#endif
