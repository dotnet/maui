using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1601 : _IssuesUITest
	{
		public Issue1601(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Exception thrown when `Removing Content Using LayoutCompression";

		[Test]
		[Category(UITestCategories.Layout)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void Issue1601Test()
		{
			App.Screenshot("Start G1601");
			App.WaitForElement("CrashButton");
			App.Screenshot("I see the button");
			App.Tap("CrashButton");
			App.Screenshot("Didn't crash!");
		}
	}
}