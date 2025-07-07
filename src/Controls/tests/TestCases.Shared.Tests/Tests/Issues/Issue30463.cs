using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30463 : _IssuesUITest
{
	public Issue30463(TestDevice device) : base(device) { }

	public override string Issue => "Picker title is not displayed again";

	[Test]
	[Category(UITestCategories.Picker)]
	public void PickerShouldRegainTitle()
	{
		App.WaitForElement("ChangeSelectedIndexToMinusOne");
		App.Tap("ChangeSelectedIndexToMinusOne");
		var picker = App.WaitForElement("RegainingPickerTitle").GetText();
#if WINDOWS || IOS
			Assert.That(picker, Is.Empty);
#else
		Assert.That(picker, Is.EqualTo("Select an item"));
#endif
	}
}