#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST
using System;
using System.Threading;
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
		Assert.That(App.WaitForElement("CurrentValue").GetText(), Is.EqualTo("Current: 1"));
		Assert.That(App.WaitForElement("ItemsValue").GetText(), Is.EqualTo("Items: 0,1,2"));
		AssertItemIsCentered("CarouselItem1");
		App.Tap("RemoveCurrentItem");
		App.WaitForTextToBePresentInElement("ItemsValue", "Items: 0,2");
		App.WaitForTextToBePresentInElement("CurrentValue", "Current: 2");
		Thread.Sleep(1000);
		Assert.That(App.WaitForElement("CurrentValue").GetText(), Is.EqualTo("Current: 2"));
		AssertItemIsCentered("CarouselItem2");
	}

	void AssertItemIsCentered(string automationId)
	{
		var carouselRect = App.WaitForElement("Carousel").GetRect();
		var itemRect = App.WaitForElement(automationId).GetRect();
		var carouselCenterX = carouselRect.X + carouselRect.Width / 2f;
		var itemCenterX = itemRect.X + itemRect.Width / 2f;
		Assert.That(Math.Abs(itemCenterX - carouselCenterX), Is.LessThanOrEqualTo(5f), $"{automationId} should be the rendered current item");
	}
}
#endif
