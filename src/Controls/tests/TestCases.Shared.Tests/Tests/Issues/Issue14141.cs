#if TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/31670
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14141 : _IssuesUITest
{
	public Issue14141(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Incorrect Intermediate CurrentItem updates with CarouselView Scroll Animation Enabled";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCurrentItemUpdatesWithScrollAnimation()
	{
		App.WaitForElement("Issue14141ScrollBtn");
		App.Tap("Issue14141ScrollBtn");
		VerifyScreenshot();
	}
}
#endif