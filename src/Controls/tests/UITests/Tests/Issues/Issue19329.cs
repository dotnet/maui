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
			IUIElement button = App.FindElement("TapHere");
			Rectangle rect = button.GetRect();

			App.TapCoordinates(rect.CenterX(), rect.CenterY());
			//App.Click(rect.CenterX(), rect.CenterY());
			App.WaitForElement("TapAccepted");
			App.WaitForElement("Success");
		}
	}
}