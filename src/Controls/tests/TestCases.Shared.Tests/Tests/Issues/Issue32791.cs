using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32791 : _IssuesUITest
{
	public Issue32791(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Setting the IsEnabled property of the CarouselView to false does not prevent swiping through items";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewPreventsSwipingWhenDisabled()
	{
		App.WaitForElement("DisabledCarouselView");
		App.ScrollRight("DisabledCarouselView");

#if MACCATALYST
		// MacCatalyst: Wait for item transition to complete after scroll gesture
		Thread.Sleep(1000);
#endif
		var statusText = App.WaitForElement("Issue32791StatusLabel").GetText();
		Assert.That(statusText, Is.EqualTo("Success"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyCollectionViewPreventsScrollingWhenDisabled()
	{
		App.WaitForElement("DisabledCollectionView");
		App.ScrollDown("DisabledCollectionView");

		var statusText = App.WaitForElement("Issue32791CollectionViewStatusLabel").GetText();
		Assert.That(statusText, Is.EqualTo("Success"));
	}
}
