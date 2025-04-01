using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla41600 : _IssuesUITest
	{
		const string BtnScrollToNonExistentItem = "btnScrollToNonExistentItem";
		const string BtnScrollToExistentItem = "btnScrollToExistentItem";
		const string FirstListItem = "0";
		const string MiddleListItem = "15";

		public Bugzilla41600(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Invalid item param value for ScrollTo throws an error";

		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla41600Test()
		{
			App.WaitForElement(BtnScrollToNonExistentItem);
			App.WaitForElement(BtnScrollToExistentItem);

			App.Tap(BtnScrollToNonExistentItem);
			App.WaitForElement(FirstListItem);

			App.Tap(BtnScrollToExistentItem);
			App.WaitForElementTillPageNavigationSettled(MiddleListItem);
		}
	}
}