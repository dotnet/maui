using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class RefreshViewTests : _IssuesUITest
	{
		public RefreshViewTests(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Refresh View Tests";

		[Fact]
		[Trait("Category", UITestCategories.RefreshView)]
		public void IsRefreshingAndCommandTest()
		{
			App.WaitForElement("ToggleRefresh");
			App.Tap("ToggleRefresh");
			Assert.True(App.WaitForTextToBePresentInElement("IsRefreshingLabel", "IsRefreshing: True"));
			Assert.True(App.WaitForTextToBePresentInElement("IsRefreshingLabel", "IsRefreshing: False"));
		}
	}
}
