using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Trait("Category", UITestCategories.ScrollView)]
	public class ScrollViewNoContentUITests : _IssuesUITest
	{
		public ScrollViewNoContentUITests(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Scrollview with null content crashes on Windows";

		// NullContentOnScrollViewDoesntCrash (src\Compatibility\ControlGallery\src\Issues.Shared\Issue3507.cs)
		[Fact]
		public void ScrollViewNoContentTest()
		{
			// 1. If the ScrollView is created without having Content, the test has passed.
			App.WaitForNoElement("Success");
		}
	}
}