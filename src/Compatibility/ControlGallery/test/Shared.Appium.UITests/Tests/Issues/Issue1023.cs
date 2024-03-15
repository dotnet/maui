#if IOS
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1023 : IssuesUITest
	{
		public Issue1023(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Exception Ancestor must be provided for all pushes except first";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Picker)]
		[FailsOnIOS]
		public void Bugzilla1023Test()
		{
			for (var n = 0; n < 10; n++)
			{
				RunningApp.WaitForElement("Push");
				RunningApp.Tap("Push");

				RunningApp.WaitForElement("ListView");
				RunningApp.Back();
			}

			// At this point, the counter can be any value, but it's most likely not zero.
			// Invoking GC once is enough to clean up all garbage data and set counter to zero
			RunningApp.WaitForElement("GC");
			RunningApp.Tap("GC");

			RunningApp.WaitForNoElement("Counter: 0");
		}
	}
}
#endif