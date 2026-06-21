#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS //The Position property now functions correctly on Android and Windows, issue: https://github.com/dotnet/maui/issues/15443. // This scenario fails on iOS,Android and Catalyst with KeepItemsInView , However carouselview flickers with KeepScrollOffset , issue:https://github.com/dotnet/maui/issues/27773.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8964 : _IssuesUITest
{
	public Issue8964(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Adding an item to the beginning of the bound ItemSource causes the carousel to skip sometimes";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void Issue8964Test()
	{
		App.WaitForElement($"Item Position - 4");
		var rect = App.WaitForElement("carouseView").GetRect();
		SwipePreviousItem(rect);
		App.WaitForElement($"Item Position - 4");
		SwipePreviousItem(rect);
		App.WaitForElement($"Item Position - 4");
		SwipePreviousItem(rect);
		App.WaitForElement($"Item Position - 4");
		SwipePreviousItem(rect);
		App.WaitForElement($"Item Position - 4");
		SwipePreviousItem(rect);
		App.WaitForElement(	$"Item Position - 4");
		App.WaitForElement($"Counter 6");
	}
 
	void SwipePreviousItem(System.Drawing.Rectangle rect)
	{
#if ANDROID
		App.DragCoordinates(rect.X + 10, rect.Y, rect.X + rect.Width - 10, rect.Y);
#else
		App.SwipeLeftToRight("carouseView");
#endif
	}	
}
#endif