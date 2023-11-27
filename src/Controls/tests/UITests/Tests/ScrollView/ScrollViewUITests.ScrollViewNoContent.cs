using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
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

			App.WaitForNoElement("Success");
		}
	}
}