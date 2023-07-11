using Microsoft.Maui.Appium;
using Microsoft.Maui.AppiumTests;
using NUnit.Framework;

namespace Controls.AppiumTests.Tests.Issues
{
	public class RefreshViewTests : _IssuesUITest
	{
		public RefreshViewTests(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Refresh View Tests";

		[Test]
		public void IsRefreshingAndCommandTest()
		{
			App.Tap("ToggleRefresh");
			Assert.IsTrue(App.WaitForTextToBePresentInElement("IsRefreshingLabel", "IsRefreshing: True"));
			Assert.IsTrue(App.WaitForTextToBePresentInElement("IsRefreshingLabel", "IsRefreshing: False"));
		}
	}
}
