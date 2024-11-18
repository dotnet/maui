using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2035 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue2035(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "App crashes when setting CurrentPage on TabbedPage in ctor in 2.5.1pre1";

		[Test]
		[Category(UITestCategories.TabbedPage)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void Issue2035Test()
		{
			App.WaitForElement(Success);
			//if it doesn't crash, we're good.
		}
	}
}
