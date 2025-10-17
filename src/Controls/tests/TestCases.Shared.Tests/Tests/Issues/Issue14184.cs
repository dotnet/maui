#if TEST_FAILS_ON_WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14184 : _IssuesUITest
{
	public Issue14184(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Setting the IsEnabled property of the CarouselView to false does not prevent swiping through items";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewPreventsSwipingWhenDisabled()
	{
		App.WaitForElement("DisabledCarouselView");
		App.ScrollRight("DisabledCarouselView");

		var statusText = App.WaitForElement("Issue14184StatusLabel").GetText();
		Assert.That(statusText, Is.EqualTo("Success"));
	}
}
#endif