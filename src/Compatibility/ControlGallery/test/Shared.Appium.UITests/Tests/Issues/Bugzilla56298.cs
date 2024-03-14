using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Bugzilla56298 : IssuesUITest
	{
		public Bugzilla56298(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Changing ListViews HasUnevenRows at runtime on iOS has no effect";

		[Test]
		[Category(UITestCategories.ListView)]
		[FailsOnIOS]
		public void Bugzilla56298Test()
		{
			RunningApp.WaitForElement("btnAdd");
			RunningApp.Tap("btnAdd");
			RunningApp.Tap("btnToggle");
			RunningApp.Screenshot("Verify we see uneven rows");
		}
	}
}