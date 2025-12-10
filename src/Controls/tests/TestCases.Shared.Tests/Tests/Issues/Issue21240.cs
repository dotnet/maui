#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21240 : _IssuesUITest
	{
		public override string Issue => "FlyoutPage IsGestureEnabled not working";

		public Issue21240(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.FlyoutPage)]
		public void FlyoutShouldNotBePresented()
		{
			App.WaitForElement("label");
			App.SwipeLeftToRight(1, 500);

			// The test passes if a flyout is not present
			VerifyScreenshot();
		}
	}
}
#endif