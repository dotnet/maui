#if !MACCATALYST && !IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.ScrollView)]
	public class ScrollViewDelayedContentUITests : _IssuesUITest
	{
		public ScrollViewDelayedContentUITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "Crash measuring empty ScrollView";

		// MeasuringEmptyScrollViewDoesNotCrash (src\Compatibility\ControlGallery\src\Issues.Shared\Issue1538.cs)
		[Test]
		[Description("Measuring empty ScrollView does not crash")]
		[FailsOnIOSWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnMacWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		public async Task MeasuringEmptyScrollViewDoesNotCrash()
		{
			// 1. Let's add a child after a second.
			await Task.Delay(1000);

			// 2. Verify that the child has been added without exceptions.
			App.WaitForNoElement("Foo");
		}
	}
}
#endif