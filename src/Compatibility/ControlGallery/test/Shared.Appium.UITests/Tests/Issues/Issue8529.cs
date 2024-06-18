/*
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue8529 : IssuesUITest
	{
		const string ButtonId = "ButtonId";

		public Issue8529(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] [Shell] iOS - BackButtonBehavior Command property binding throws InvalidCastException when using a custom command class that implements ICommand";
		
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.Shell)]
		public void Issue8529ShellBackButtonBehaviorCommandPropertyCanUseICommand()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(ButtonId, "Timed out waiting for first page.");
			RunningApp.Tap(ButtonId);
			RunningApp.WaitForElement("LabelId", "Timed out waiting for the destination page.");
			RunningApp.Back();
			RunningApp.WaitForElement(ButtonId, "Timed out waiting to navigate back to the first page.");
		}
	}
}
*/