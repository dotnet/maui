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
			Task.Delay(2000);
			App.DragCoordinates(0,0,0,120);
			Task.Delay(2000);
			VerifyScreenshot();
		}
	}
}
