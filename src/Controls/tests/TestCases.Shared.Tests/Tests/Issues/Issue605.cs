#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue605 : _IssuesUITest
{
	public Issue605(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Allow text selection and copy in readonly Entry";

	[Test]
	[Category(UITestCategories.Entry)]
	public void TextSelectionShouldBeEnabledInReadonlyEditor()
	{
		App.WaitForElement("Entry");
		App.LongPress("Entry");
		VerifyScreenshot();
	}
}
#endif