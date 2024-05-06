using System.Drawing;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19329 : _IssuesUITest
	{
		public Issue19329(TestDevice device) : base(device) { }

		public override string Issue => "Pointer gestures should work with relative positions correctly";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void RelativePointerPositionIsComputedCorrectly()
		{
			_ = App.WaitForElement("TapHere");

			App.Click("TapHere");

			App.WaitForElement("TapAccepted");
			App.WaitForElement("Success");
		}
	}
}