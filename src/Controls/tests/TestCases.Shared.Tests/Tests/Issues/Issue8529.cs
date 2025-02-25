using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8529 : _IssuesUITest
	{
		const string ButtonId = "ButtonId";

		public Issue8529(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] [Shell] iOS - BackButtonBehavior Command property binding throws InvalidCastException when using a custom command class that implements ICommand";

		[Test]
		[Category(UITestCategories.Shell)]
		public void Issue8529ShellBackButtonBehaviorCommandPropertyCanUseICommand()
		{

			App.WaitForElement(ButtonId, "Timed out waiting for first page.");
			App.Tap(ButtonId);
			App.WaitForElement("LabelId", "Timed out waiting for the destination page.");
			App.TapBackArrow("BackButtonImage");
			App.WaitForElement(ButtonId, "Timed out waiting to navigate back to the first page.");
		}
	}
}