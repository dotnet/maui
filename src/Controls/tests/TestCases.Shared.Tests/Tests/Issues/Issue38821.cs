#if TEST_FAILS_ON_WINDOWS // https://github.com/dotnet/maui/issues/36230
using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38821 : _IssuesUITest
{
	public Issue38821(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CarouselView resets CurrentItem after removing the current item";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void RemovingCurrentItemThenSelectingAnotherItemPreservesCurrentItem()
	{
		Assert.That(
		 App.WaitForElement("CurrentValue").GetText(),
		 Is.EqualTo("Current: 1"));

		AssertItemIsCentered("CarouselItem1");

		App.Tap("RemoveCurrentItem");

		App.WaitForTextToBePresentInElement("CurrentValue", "Current: 2");

		Assert.That(
		 App.WaitForElement("CurrentValue").GetText(),
		 Is.EqualTo("Current: 2"));

		AssertItemIsCentered("CarouselItem2");
	}

	void AssertItemIsCentered(string automationId)
	{
		var carouselRect = App.WaitForElement("Carousel").GetRect();
		var itemRect = App.WaitForElement(automationId).GetRect();

		var carouselCenterX = carouselRect.X + carouselRect.Width / 2f;
		var itemCenterX = itemRect.X + itemRect.Width / 2f;

		Assert.That(
		 Math.Abs(itemCenterX - carouselCenterX),
		 Is.LessThanOrEqualTo(5f),
		 $"{automationId} should be the rendered current item");
	}
}
#endif
