using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32310 : _IssuesUITest
{
	public Issue32310(TestDevice device) : base(device)
	{
	}

	public override string Issue => "App hangs if PopModalAsync is called after PushModalAsync with single await Task.Yield()";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void ModalNavigationShouldNotHang()
	{
		App.WaitForElement("NavigateButton");
		App.Tap("NavigateButton");

		// If the fix works, the modal should push and pop quickly,
		// and we should be back to the same page with the button visible
		App.WaitForElement("NavigateButton", timeout: TimeSpan.FromSeconds(5));
	}
}