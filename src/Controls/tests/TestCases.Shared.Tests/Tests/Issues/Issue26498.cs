using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26498 : _IssuesUITest
	{
		public override string Issue => "Null Exception on clearing collection in list view after navigation";
		public Issue26498(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.ListView)]
		public void Issue26498TestNullException()
		{
			const string OpenListViewPage = "OpenListViewPage";
			const string ClearButton = "ClearButton";
			const string BackButton = "BackButton";

			App.WaitForElement(OpenListViewPage);
			App.Tap(OpenListViewPage);
			App.WaitForElement(BackButton);
			App.Click(BackButton);
			App.WaitForElement(OpenListViewPage);
			App.Click(OpenListViewPage);
			App.WaitForElement(ClearButton);
			App.Click(ClearButton);
			App.WaitForElement(ClearButton);
		}
	}
}
