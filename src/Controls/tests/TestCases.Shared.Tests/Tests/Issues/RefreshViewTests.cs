using NUnit.Framework;
using NUnit.Framework.Legacy;
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

		[Test]
		[Category(UITestCategories.RefreshView)]
		public void IsRefreshingAndCommandTest()
		{
			App.WaitForElement("ToggleRefresh");
			App.Tap("ToggleRefresh");
			ClassicAssert.IsTrue(App.WaitForTextToBePresentInElement("IsRefreshingLabel", "IsRefreshing: True"));
			ClassicAssert.IsTrue(App.WaitForTextToBePresentInElement("IsRefreshingLabel", "IsRefreshing: False"));
		}
	}
}
