using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32287 : _IssuesUITest
{
	public Issue32287(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Using custom TitleView in AppShell causes shell content to be covered in iOS 26";

	[Test]
	[Category(UITestCategories.Shell)]
	public void CustomTitleViewDoesNotCoverContent()
	{
		App.WaitForElement("Label");
		VerifyScreenshot();
	}
}
