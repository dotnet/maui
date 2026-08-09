using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29261 : _IssuesUITest
{
	public override string Issue => "CarouselViewHandler2 for iOS does not properly bounce back when reaching the end with Loop=false";

	public Issue29261(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewBounceTest()
	{
		App.WaitForElement("carouselview");

		// Use button to navigate to the last position
		App.Tap("goToLastButton");
		App.WaitForTextToBePresentInElement("positionLabel", "CarouselView Position - 2");

		// Swipe once more past the end — should bounce back to position 2
		App.ScrollRight("carouselview", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollRight("carouselview", ScrollStrategy.Gesture, 0.9, 500);
		var text = App.FindElement("positionLabel").GetText();
		Assert.That(text, Is.EqualTo("CarouselView Position - 2"));

	}
}