using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla53445 : _IssuesUITest
	{
		public Bugzilla53445(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Setting Grid.IsEnabled to false does not disable child controls";

		[Test]
		[Category(UITestCategories.Layout)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		[FailsOnMac]
		public void Test()
		{
			App.WaitForElement("Success");

			// Disable the layouts
			App.Tap("toggle");

			// Tap the grid button; the event should not fire and the label should not change
			App.Tap("gridbutton");
			App.WaitForElement("Success");

			// Tap the contentview button; the event should not fire and the label should not change
			App.Tap("contentviewbutton");
			App.WaitForElement("Success");

			// Tap the stacklayout button; the event should not fire and the label should not change
			App.Tap("stacklayoutbutton");
			App.WaitForElement("Success");
		}
	}
}