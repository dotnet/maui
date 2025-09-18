using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue25634 : _IssuesUITest
{
	public Issue25634(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Picker ItemDisplayBinding doesn't support MVVM properly";

	[Test]
	[Category(UITestCategories.Picker)]
	public void VerifyPickerItemDisplayBindingValue()
	{
		App.WaitForElement("PickerButton");
		App.Tap("PickerButton");
		VerifyScreenshot();
	}
}