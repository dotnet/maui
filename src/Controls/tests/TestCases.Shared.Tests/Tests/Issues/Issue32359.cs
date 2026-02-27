using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue32359 : _IssuesUITest
	{
		public override string Issue => "FlowDirection RightToLeft not applied to CollectionView with VerticalGrid multi-column layout";

		public Issue32359(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerticalGridCollectionViewRTLColumnMirroringShouldWork()
		{
			// Wait for CollectionView to load
			App.WaitForElement("TestCollectionView");
			App.WaitForElement("SetRtlButton");
			
			// Set to RTL - this should mirror the column order
			// In a 4-column grid, items should appear: 4, 3, 2, 1 (right to left)
			// instead of: 1, 2, 3, 4 (left to right)
			App.Tap("SetRtlButton");
			
			// Wait for layout to update
			App.WaitForElement("StatusLabel");
			
			// Verify screenshot shows RTL column mirroring
			// The first row should show: "4  3  2  1" (from right to left)
			// instead of: "1  2  3  4" (from left to right)
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerticalGridCollectionViewLTRToRTLToggleShouldWork()
		{
			// Start in LTR (default)
			App.WaitForElement("TestCollectionView");
			App.WaitForElement("SetLtrButton");
			
			// Verify LTR layout first (optional baseline)
			App.Tap("SetLtrButton");
			App.WaitForElement("StatusLabel");
			
			// Toggle to RTL
			App.Tap("SetRtlButton");
			App.WaitForElement("StatusLabel");
			
			// Toggle back to LTR
			App.Tap("SetLtrButton");
			App.WaitForElement("StatusLabel");
			
			// Toggle to RTL again - ensure it still works
			App.Tap("SetRtlButton");
			App.WaitForElement("StatusLabel");
			
			// Final verification in RTL mode
			VerifyScreenshot();
		}
	}
}
