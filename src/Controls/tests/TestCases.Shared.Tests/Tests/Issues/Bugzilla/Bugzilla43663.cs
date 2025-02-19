using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla43663 : _IssuesUITest
{
	const string AlertCancelButton = "Cancel";

	public Bugzilla43663(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ModalPushed and ModalPopped not working on WinRT";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void ModalNavigation()
	{
		App.WaitForElement("Click to push Modal");
		App.Tap("Click to push Modal");
		App.TapDisplayAlertButton(AlertCancelButton);
		App.WaitForElement("Modal");
		App.Tap("Click to dismiss modal");
		App.TapDisplayAlertButton(AlertCancelButton);
		App.WaitForElement("Click to push Modal");
	}
}