#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28524 : _IssuesUITest
{
	public Issue28524(TestDevice device) : base(device) { }

	public override string Issue => "[iOS] CurrentItem does not work when PeekAreaInsets is set";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void CurrentItemShouldWork()
	{
		App.WaitForElement("CarouselView");
		App.ScrollRight("CarouselView");
		App.ScrollRight("CarouselView");
		App.WaitForElement("Blue Monkey");
	}
}
#endif