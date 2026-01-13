#if TEST_FAILS_ON_WINDOWS // Related issue for windows: https://github.com/dotnet/maui/issues/29245
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29524 : _IssuesUITest
{
	public override string Issue => "Previous Position in PositionChangedEventArgs Not Updating Correctly in CarouselView";

	public Issue29524(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyPreviousPositionUpdatesCorrectlyWhenInsertingItems()
	{
		App.WaitForElement("carouselview");
		App.Tap("InsertButton");
		var text = App.FindElement("positionLabel").GetText();
		Assert.That(text, Is.EqualTo("Current Position: 0, Previous Position: 1"));
	}

#if TEST_FAILS_ON_ANDROID  // Related issue for android: https://github.com/dotnet/maui/issues/29415
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyPreviousPositionUpdatesCorrectlyWhenRemovingItems()
	{
		App.WaitForElement("carouselview");
		App.Tap("RemoveButton");
		var text = App.FindElement("positionLabel").GetText();
		Assert.That(text, Is.EqualTo("Current Position: 0, Previous Position: 2"));
	}
#endif
}
#endif