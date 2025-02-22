#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23563 : _IssuesUITest
	{
		public Issue23563(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Unable to set toolbar overflow menu color if not using shell";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void OverflowIconShouldBeRed()
		{
			App.WaitForElement("Label");
			VerifyScreenshot();
		}
	}
}
#endif