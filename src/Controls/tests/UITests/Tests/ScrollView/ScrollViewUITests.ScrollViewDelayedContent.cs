using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	public class ScrollViewDelayedContentUITests : ScrollViewUITests
	{
		public ScrollViewDelayedContentUITests(TestDevice device)
			: base(device)
		{
		}

		// MeasuringEmptyScrollViewDoesNotCrash (src\Compatibility\ControlGallery\src\Issues.Shared\Issue1538.cs)
		[Test]
		[Description("Measuring empty ScrollView does not crash")]
		public async Task MeasuringEmptyScrollViewDoesNotCrash()
		{
			await Task.Delay(1000);
			App.WaitForNoElement("Foo");
		}
	}
}