#if TEST_FAILS_ON_WINDOWS //Issue Link - https://github.com/dotnet/maui/issues/28619
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35764 : _IssuesUITest
{
	public Issue35764(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "[Android] SearchHandler.ClearPlaceholderEnabled has no effect";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ClearPlaceholderIconShouldHideWhenDisabled()
	{
		App.WaitForElement("ToggleClearPlaceholderEnabled");
		App.Tap("ToggleClearPlaceholderEnabled");
		VerifyScreenshot();
	}
}
#endif
