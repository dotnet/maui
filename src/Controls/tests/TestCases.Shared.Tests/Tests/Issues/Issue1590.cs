using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1590 : _IssuesUITest
	{
		public Issue1590(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ListView.IsGroupingEnabled results ins ArguementOutOfRangeException";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void ListViewIsGroupingEnabledDoesNotCrash()
		{
			App.WaitForNoElement("First");
		}
	}
}