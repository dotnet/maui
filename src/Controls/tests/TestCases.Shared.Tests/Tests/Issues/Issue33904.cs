using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33904 : _IssuesUITest
{
	public Issue33904(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CharacterSpacing applied to Label is not inherited by Span elements in FormattedString";

	[Test]
	[Category(UITestCategories.Label)]
	public void VerifySpanInheritsLabelCharacterSpacing()
	{
		App.WaitForElement("InheritedCharacterSpacingLabel");
		VerifyScreenshot();
	}
}