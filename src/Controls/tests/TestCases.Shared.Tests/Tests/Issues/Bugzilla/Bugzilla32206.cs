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
		[Category(UITestCategories.ContextActions)]
		public void Bugzilla32206Test()
		{
			for (var n = 0; n < 10; n++)
			{
				App.WaitForElement("Push");
				App.Tap("Push");
				App.WaitForElement("ListView");
				App.TapBackArrow();
			}
			App.WaitForElement("GC");
			App.Tap("GC");
			App.WaitForElement("Counter: 0");
		}
	}
}
