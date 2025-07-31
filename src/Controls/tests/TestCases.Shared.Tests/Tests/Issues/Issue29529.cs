using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29529 : _IssuesUITest
{
	public override string Issue => "CurrentItemChangedEventArgs and PositionChangedEventArgs Not Updating Correctly in CarouselView";

	public Issue29529(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void Issue29529VerifyPreviousPositionOnInsert()
	{
		App.WaitForElement("carouselview");
		App.Tap("InsertButton");
		var text = App.FindElement("positionLabel").GetText();
		Assert.That(text, Is.EqualTo("Current Position: 0, Previous Position: 1"));
		text = App.FindElement("itemLabel").GetText();
		Assert.That(text, Is.EqualTo("Current Item: Item 0, Previous Item: Item 1"));
	}
}