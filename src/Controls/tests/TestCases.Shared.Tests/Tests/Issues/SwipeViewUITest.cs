using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class SwipeViewUITest : _IssuesUITest
	{
		public override string Issue => "SwipeView UI Test";

		public SwipeViewUITest(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.SwipeView)]
		public void VerifySwipeViewApperance()
		{
			var rect = App.WaitForElement("SwipeRight").GetRect();
			var centerX = rect.X + rect.Width / 2;
			var centerY = rect.Y + rect.Height / 2;
			App.DragCoordinates(centerX, centerY, centerX - 200, centerY);
			VerifyScreenshot();
		}
	}
}