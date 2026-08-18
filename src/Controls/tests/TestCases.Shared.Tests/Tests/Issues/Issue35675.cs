#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // Android Issue: https://github.com/dotnet/maui/issues/35643, Windows PR: https://github.com/dotnet/maui/pull/35398
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35675 : _IssuesUITest
{
	public Issue35675(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[iOS] CarouselView freezes with infinite loop when IsScrollAnimated=False";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void CV2DoesNotFreezeWhenSettingCurrentItemWithIsScrollAnimatedFalse()
	{
		App.WaitForElement("ScrollButton");
		App.WaitForElement("Item 0");
		App.Tap("ScrollButton");

		App.WaitForElement("Item 2b");
	}
}
#endif
