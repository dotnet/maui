#if IOS || ANDROID // SafeAreaEdges not supported on Catalyst and Windows
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue34563 : _IssuesUITest
	{
		public override string Issue => "[iOS] Vertical SafeAreaEdge isn't respected when Top and Bottom constraints mismatch";

		public Issue34563(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.SafeAreaEdges)]
		public void SafeAreaEdgesRespectedWhenTopAndBottomMismatch()
		{
			App.WaitForElement("RootGrid");

			var topSafeBoxRect = App.WaitForElement("TopSafeBox").GetRect();
			Assert.That(topSafeBoxRect.Y, Is.GreaterThan(0),
				"TopSafeBox opted into SafeAreaEdges.Container and should be inset from the top of the screen, " +
				"even though the Page's Top edge is None.");

			var bottomSafeBoxRect = App.WaitForElement("BottomSafeBox").GetRect();
			var rootGridRect = App.WaitForElement("RootGrid").GetRect();
			var screenBottom = rootGridRect.Y + rootGridRect.Height;
			var bottomSafeBoxBottom = bottomSafeBoxRect.Y + bottomSafeBoxRect.Height;

			Assert.That(bottomSafeBoxBottom, Is.LessThanOrEqualTo(screenBottom),
				"BottomSafeBox should not extend past the bottom of its container.");
		}
	}
}
#endif
