using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1906 : _IssuesUITest
	{
		public Issue1906(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "AlertView doesn't scroll when text is too large";

		[Fact]
		[Trait("Category", UITestCategories.DisplayAlert)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void TestIssue1906()
		{
			App.WaitForElement("Show alert");
			App.Tap("Show alert");
			App.WaitForElement("Ok");
			App.Tap("Ok");
		}
	}
}
