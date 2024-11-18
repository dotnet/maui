#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18857 : _IssuesUITest
	{
		public Issue18857(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "ImageButton Padding & Ripple effect stops working with .NET 8";

		[Test]
		[Category(UITestCategories.ImageButton)]
		[FailsOnMacWhenRunningOnXamarinUITest("VerifyScreenshot method not implemented")]
		public async Task GradientImageButtonBackground()
		{
			App.WaitForElement("TestImageButton");

			App.Tap("TestRemoveBackgroundButton");
			App.Tap("TestUpdateBackgroundButton");

			App.WaitForElement("TestImageButton");

			await Task.Yield(); // Wait for Ripple Effect animation to complete.

			VerifyScreenshot();
		}
	}
}
#endif