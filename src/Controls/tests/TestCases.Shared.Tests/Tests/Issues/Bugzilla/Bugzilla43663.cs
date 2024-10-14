#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla43663 : _IssuesUITest
{
	const string PushModal = "Push Modal";

	const string PopModal = "Pop Modal";

	const string Modal = "Modal";

	public Bugzilla43663(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ModalPushed and ModalPopped not working on WinRT";

	// [Test]
	// [Category(UITestCategories.Navigation)]
	// public void ModalNavigation()
	// {
	// 	var i = 0;
	// 	while(App.GetAlerts().Count == 0 && i < 3)
	// 	{
	// 		i++;
	// 		Task.Delay(1000);
	// 	}

	// 	App.GetAlert()?.DismissAlert();
	// 	App.WaitForElement(PushModal);
	// 	App.Tap(PushModal);
	// 	App.GetAlert()?.DismissAlert();
	// 	App.WaitForElement(Modal);
	// 	App.Tap(PopModal);
	// 	App.GetAlert()?.DismissAlert();
	// 	App.WaitForElement(PushModal);
	// }
}
#endif