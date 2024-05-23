using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla59097 : _IssuesUITest
	{
		public Bugzilla59097(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Calling PopAsync via TapGestureRecognizer causes an application crash";
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.Gestures)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla59097Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("boxView");
			App.Tap("boxView");
		}
	}
}