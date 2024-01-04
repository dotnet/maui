using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
using System.Drawing;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19329 : _IssuesUITest
	{
		public Issue19329(TestDevice device) : base(device) { }

		public override string Issue => "Pointer gestures should work with relative positions correctly";

		[Test]
		public void RelativePointerPositionIsComputedCorrectly()
		{
			IUIElement boxView = App.WaitForElement("TapHere");
			Rectangle rect = boxView.GetRect();

			App.TapCoordinates(rect.CenterX(), rect.CenterY());

			App.WaitForElement("TapAccepted");
			App.WaitForElement("Success");
		}
	}
}