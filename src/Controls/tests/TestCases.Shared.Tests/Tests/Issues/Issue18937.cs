using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18937 : _IssuesUITest
	{
		public Issue18937(TestDevice device) : base(device) { }

		public override string Issue => "[Windows] Setting IsClippedToBound is true inside a Border control will crash on Windows";

		[Fact]
		[Category(UITestCategories.Border)]
		public void ExceptionShouldNotOccurWhenIsClippedToBoundsIsTrue()
		{
			var testLabel = App.WaitForElement("Label");
			Assert.Equal("Label Inside the Border", testLabel.GetText());
		}
	}
}