using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3390 : _IssuesUITest
	{
		public Issue3390(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Crash or incorrect behavior with corner radius 5";

		[Fact]
		[Category(UITestCategories.Button)]
		[Category(UITestCategories.Compatibility)]
		public void Issue3390Test()
		{
			App.WaitForElement("TestButton");
			App.Tap("TestButton");
			Assert.Equal("Success", App.FindElement("TestButton").GetText());
		}
	}
}