using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla56298 : _IssuesUITest
	{
		public Bugzilla56298(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Changing ListViews HasUnevenRows at runtime on iOS has no effect";

		[Test]
		[Category(UITestCategories.ListView)]
		public void Bugzilla56298Test()
		{
			App.WaitForElement("btnAdd");
			App.Tap("btnAdd");
			App.WaitForElement("btnToggle");
			App.Tap("btnToggle");
			App.WaitForElement("btnAdd");
			VerifyScreenshot();
		}
	}
}