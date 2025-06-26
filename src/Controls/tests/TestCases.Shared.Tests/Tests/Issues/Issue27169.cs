using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27169(TestDevice device) : _IssuesUITest(device)
	{
		public override string Issue => "Grid inside ScrollView should measure with infinite constraints";

		[Fact]
		[Trait("Category", UITestCategories.ScrollView)]
		public void ScrollViewContentLayoutMeasuresWithInfiniteConstraints()
		{
			var measuredHeight = App.WaitForElement("StubLabel").GetText()!;
			Assert.Equal("200", measuredHeight);
		}
	}
}