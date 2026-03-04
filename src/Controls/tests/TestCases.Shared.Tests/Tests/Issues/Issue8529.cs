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
			App.WaitForElementTillPageNavigationSettled("LabelId");
			if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			{
				App.Back(); // In iOS 26, the AutomationID set on the Back button is not found by Appium, so the Back method is used for iOS 26.
			}
			else
			{
				App.TapBackArrow("BackButtonImage");
			}
			App.WaitForElement(ButtonId, "Timed out waiting to navigate back to the first page.");
		}
	}
}