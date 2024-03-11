using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla53445 : IssuesUITest
	{
		public Bugzilla53445(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Setting Grid.IsEnabled to false does not disable child controls";

		[Test]
		[Category(UITestCategories.Layout)]
		public void Test()
		{
			RunningApp.WaitForElement("Success");

			// Disable the layouts
			RunningApp.Tap("toggle");

			// Tap the grid button; the event should not fire and the label should not change
			RunningApp.Tap("gridbutton");
			RunningApp.WaitForElement("Success");

			// Tap the contentview button; the event should not fire and the label should not change
			RunningApp.Tap("contentviewbutton");
			RunningApp.WaitForElement("Success");

			// Tap the stacklayout button; the event should not fire and the label should not change
			RunningApp.Tap("stacklayoutbutton"));
			RunningApp.WaitForElement("Success");
		}
	}
}