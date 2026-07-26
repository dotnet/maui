using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue35943 : _IssuesUITest
	{
		public Issue35943(TestDevice device) : base(device) { }

		public override string Issue => "[iOS, MacCatalyst] GetPosition Truncates Fractional Coordinates to Integers on TappedEvent";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void GetPositionPreservesFractionalCoordinates()
		{
			// The tap target is a BoxView. The reference box (ReferenceBox) has a 0.5-point
			// margin, placing it at a fractional UIKit coordinate. GetPosition(relativeTo: ReferenceBox)
			// should therefore return coordinates with a fractional component.
			// Before the fix, an explicit (int) cast in CalculatePosition truncated these values.
			var tapRect = App.WaitForElement("TapTarget").GetRect();

			// Tap at integer screen coordinates so position relative to the 0.5-point reference box
			// is expected to include a fractional component.
			App.TapCoordinates((int)tapRect.CenterX(), (int)tapRect.CenterY());

			// "Success" appears when the coordinates have a fractional component;
			// "Failure" appears when they are truncated to integers.
			App.WaitForElement("Success");
		}
	}
}
