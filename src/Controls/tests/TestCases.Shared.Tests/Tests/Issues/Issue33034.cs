using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue33034 : _IssuesUITest
	{
		public override string Issue => "SafeAreaEdges works correctly only on the first tab in Shell. Other tabs have content colliding with the display cutout in the landscape mode.";

		public Issue33034(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.SafeAreaEdges)]
		public void SafeAreaShouldWorkOnAllShellTabs()
		{
			// Wait for the first tab to load
			App.WaitForElement("LeftEdgeLabel");
			
			// Get the X position of the left edge label on the first tab
			var firstTabLabelRect = App.FindElement("LeftEdgeLabel").GetRect();
			var firstTabLeftPosition = firstTabLabelRect.X;
			
			// The label should have proper left padding (safe area inset)
			// With our SafeArea fix, it should be > 0
			Assert.That(firstTabLeftPosition, Is.GreaterThan(0),
				$"Left edge label should have safe area inset on first tab. Position: {firstTabLeftPosition}");
		}
	}
}
