using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19191 : _IssuesUITest
{
	public override string Issue => "Picker TitleColor not working";

	public Issue19191(TestDevice device)
		: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Picker)]
	public void PickerTitleShouldBeRed()
	{
		_ = App.WaitForElement("picker");

		VerifyScreenshot();
	}
}
