#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID  // Windows Character Spacing Issue Link - https://github.com/dotnet/maui/issues/29493 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29492 : _IssuesUITest
{
	public Issue29492(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "CharacterSpacing should be applied";

	[Test]
	[Category(UITestCategories.Shell)]
	public void CharacterSpacingShouldApply()
	{
		App.WaitForElement("Entertext");
		App.Tap("Entertext");
		VerifyScreenshot();
	}
}
#endif