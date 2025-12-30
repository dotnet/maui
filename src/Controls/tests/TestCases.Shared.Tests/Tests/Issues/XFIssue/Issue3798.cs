#if TEST_FAILS_ON_WINDOWS // SepraratorColor is not supported on Windows by default. https://github.com/dotnet/maui/issues/8112
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3798 : _IssuesUITest
{
	public Issue3798(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] SeparatorColor of ListView is NOT updated dynamically";

	[Test]
	[Category(UITestCategories.ListView)]

	public void Issue3798Test()
	{
		App.WaitForElement("listViewSeparatorColor");
		App.Tap("item1");
		VerifyScreenshot();
	}
}
#endif