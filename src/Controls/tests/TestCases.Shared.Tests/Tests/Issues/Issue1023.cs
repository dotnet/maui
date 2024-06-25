#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1023 : _IssuesUITest
	{
		public Issue1023(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Exception Ancestor must be provided for all pushes except first";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Picker)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		public void Bugzilla1023Test()
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
	}
}
#endif