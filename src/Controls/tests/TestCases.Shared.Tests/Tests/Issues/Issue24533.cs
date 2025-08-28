using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24533 : _IssuesUITest
	{
		public override string Issue => "[iOS] RefreshView causes CollectionView scroll position to reset";

		public Issue24533(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.RefreshView)]
		public void CollectionViewWithRefreshViewShouldNotReset()
		{
			App.WaitForElement("Footer");
			App.Tap("Footer");
			App.ScrollTo("Footer");
			App.Tap("Footer");
			App.ScrollTo("Footer");
			App.Tap("Footer");
			VerifyScreenshot();
		}
	}
}