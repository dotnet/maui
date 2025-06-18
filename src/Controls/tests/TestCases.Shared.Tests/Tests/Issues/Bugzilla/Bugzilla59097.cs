#if TEST_FAILS_ON_WINDOWS     //BoxView automation ID isn't working on the Windows platform, causing a TimeoutException.
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

		[Test]
		[Category(UITestCategories.Gestures)]
		public void Bugzilla59097Test()
		{
			App.WaitForElement("boxView");
			App.Tap("boxView");
			App.WaitForElementTillPageNavigationSettled("previous page ");
		}
	}
}
#endif