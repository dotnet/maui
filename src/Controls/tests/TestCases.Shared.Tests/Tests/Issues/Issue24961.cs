#if TEST_FAILS_ON_CATALYST // VerifyScreenshot() method is not implemented in macOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24961 : _IssuesUITest
	{
		public Issue24961(TestDevice testDevice) : base(testDevice) { }

		public override string Issue => "[iOS] CollectionView with header and complex item will have the wrong position after a refresh with a RefreshView";

		[Test]
		[Category(UITestCategories.RefreshView)]
		public void Issue_24961()
		{
			App.WaitForElement("ContentView");
			App.DragCoordinates(150,300, 150,590);
			App.WaitForElement("IsRefreshed");
			App.Tap("StartRefresh");
			App.DragCoordinates(150,300, 151,690);
			App.WaitForElement("IsRefreshed");
			VerifyScreenshot();
		}
	}
}
#endif