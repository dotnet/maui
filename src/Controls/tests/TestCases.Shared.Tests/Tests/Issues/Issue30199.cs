using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30199 : _IssuesUITest
{
	public Issue30199(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "TimePicker CharacterSpacing Property Not Working on Windows";

	[Test]
	[Category(UITestCategories.TimePicker)]
	public void Issue30199PlaceholderCharacterSpacingShouldApply()
	{
		App.WaitForElement("timePicker");
		VerifyScreenshot();
	}
}