#if iOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue6368 : _IssuesUITest
	{
		public Issue6368(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[CustomRenderer]Crash when navigating back from page with custom renderer control";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		public void Issue6368Test()
		{
			App.WaitForElement("btnGo");
			App.Tap("btnGo");
			App.WaitForElement("GoToNextPage");
			App.Back();
		}
	}
}
#endif