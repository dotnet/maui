using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla32615 : _IssuesUITest
	{
		public Bugzilla32615(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "OnAppearing is not called on previous page when modal page is popped";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public async Task Bugzilla32615Test()
		{
			App.Tap("btnModal");
			App.Tap("btnPop");
			await Task.Delay(1000);
			App.WaitForNoElement("1");
		}
	}
}