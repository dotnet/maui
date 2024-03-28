using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class RefreshViewTests : _IssuesUITest
	{
		public RefreshViewTests(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Refresh View Tests";

		[Test]
		[Category(UITestCategories.RefreshView)]
		public void IsRefreshingAndCommandTest()
		{
			App.Tap("ToggleRefresh");
			Assert.IsTrue(App.WaitForTextToBePresentInElement("IsRefreshingLabel", "IsRefreshing: True"));
			Assert.IsTrue(App.WaitForTextToBePresentInElement("IsRefreshingLabel", "IsRefreshing: False"));
		}
	}
}
