using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30071 : _IssuesUITest
{
	public Issue30071(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "Entry Editor Placeholder CharacterSpacing Property Not Working on Windows";

	[Test]
	[Category(UITestCategories.Entry)]
	[Category(UITestCategories.Editor)]
	public void Issue30071PlaceholderCharacterSpacingShouldApply()
	{
		App.WaitForElement("label");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Entry)]
	[Category(UITestCategories.Editor)]
	public void Issue30071PlaceholderCharacterSpacingShouldChange()
	{
		App.WaitForElement("entry");
		App.WaitForElement("button");
		App.Tap("button");
		VerifyScreenshot();
	}
}
