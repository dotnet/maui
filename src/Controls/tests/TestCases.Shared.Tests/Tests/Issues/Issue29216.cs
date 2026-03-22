using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29216 : _IssuesUITest
{
	public Issue29216(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Carousel view scrolling on button click";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void Issue29216CarouselViewScrollingIssue()
	{
		App.WaitForElement("button");
		App.Tap("button");
		App.WaitForElement("Page2");
	}
}
