using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30464 : _IssuesUITest
{
	public Issue30464(TestDevice device) : base(device)
	{
	}

	public override string Issue => "The CharacterSpacing property on the Picker control is not applied to the title or the items";

	[Test]
	[Category(UITestCategories.Picker)]
	public void ValidatePickerTitleAndItemCharacterSpacing()
	{
		App.WaitForElement("Issue30464DescriptionLabel");
		App.Tap("Issue30464Btn");
		VerifyScreenshot();
	}
}