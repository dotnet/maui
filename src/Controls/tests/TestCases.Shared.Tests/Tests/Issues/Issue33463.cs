#if MACCATALYST // The reported scenario is specific to macOS, where the picker dialog closes automatically when opened using the Tab key, so this test is enabled only for macOS.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33463 : _IssuesUITest
{
	public Issue33463(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[macOS]Picker items are not visible";

	[Test]
	[Category(UITestCategories.Picker)]
	public void PickerShouldRemainOpenWhenOpenedUsingTabKey()
	{
		App.WaitForElement("TestPicker");

		App.Tap("TestPicker");
		App.WaitForElement("Done");
		App.Tap("Done");

		App.SendTabKey();
		Task.Delay(800).Wait();

		var doneButton = App.FindElement("Done");
		Assert.That(doneButton, Is.Not.Null, 
			"The picker dialog should remain open when it is opened using the Tab key.");
	}
}
#endif
