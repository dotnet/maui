using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(UITestCategories.ScrollView)]
	public class ScrollViewNoContentUITests : _IssuesUITest
	{
		public ScrollViewNoContentUITests(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Scrollview with null content crashes on Windows";

		// NullContentOnScrollViewDoesntCrash (src\Compatibility\ControlGallery\src\Issues.Shared\Issue3507.cs)
		[Test]
		[Description("ScrollView without Content no crash.")]
		public void ScrollViewNoContentTest()
		{
			// 1. If the ScrollView is created without having Content, the test has passed.
			App.WaitForNoElement("Success");
		}
	}
}