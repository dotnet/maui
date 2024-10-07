using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla32206 : _IssuesUITest
	{
		public Bugzilla32206(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ContextActions cause memory leak: Page is never destroyed";

		[Test]
		[Category(UITestCategories.Page)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		[FailsOnMac]
		[FailsOnWindows]
		public void Bugzilla32206Test()
		{
			try
			{
				for (var n = 0; n < 10; n++)
				{
					App.WaitForElement("Push");
					App.Tap("Push");

					App.WaitForElement("ListView");
					App.Back();
				}

				// At this point, the counter can be any value, but it's most likely not zero.
				// Invoking GC once is enough to clean up all garbage data and set counter to zero
				App.WaitForElement("GC");
				App.Tap("GC");

				App.WaitForNoElement("Counter: 0");
			}
			finally
			{
				App.Back();
			}
		}
	}
}