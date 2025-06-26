using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Trait("Category", UITestCategories.ScrollView)]
	public class ScrollViewObjectDisposedUITests : _IssuesUITest
	{
		public ScrollViewObjectDisposedUITests(TestDevice device)
		: base(device)
		{ }

		public override string Issue => "Object Disposed Exception in ScrollView";

		// ScrollViewObjectDisposedTest (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewObjectDisposed.cs)
		[Fact]
		public void ScrollViewObjectDisposedTest()
		{
			App.WaitForElement("TestButtonId");
			// 1. Tap the button.
			App.Tap("TestButtonId");

			// 2. Verify does not cause an exception.
			App.WaitForElement("Success");
		}
	}
}