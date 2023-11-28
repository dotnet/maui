using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(UITestCategories.ScrollView)]
	public class ScrollViewNoContentUITests : ScrollViewUITests
	{
		public ScrollViewNoContentUITests(TestDevice device)
			: base(device)
		{
		}

		// NullContentOnScrollViewDoesntCrash (src\Compatibility\ControlGallery\src\Issues.Shared\Issue3507.cs)
		[Test]
		[Description("ScrollView without Content no crash.")]
		public void ScrollViewNoContentTest()
		{
			App.Click("ScrollViewNoContent");

			// 1. If the ScrollView is created without having Content, the test has passed.
			App.WaitForNoElement("Success");
		}
	}
}