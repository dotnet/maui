#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID // Related issues have been reported for both Android and Windows : Android (https://github.com/dotnet/maui/issues/29529) and Windows (https://github.com/dotnet/maui/issues/31670)
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14141 : _IssuesUITest
{
	public Issue14141(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CarouselView scrolls to last item when Loop is enabled and CurrentItem is set to an item not in the list";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void ValidateNoScrollOnInvalidItemWithLoop()
	{
		App.WaitForElement("14141SelectBtn");
		App.Tap("14141SelectBtn");

		VerifyScreenshot();
	}
}
#endif