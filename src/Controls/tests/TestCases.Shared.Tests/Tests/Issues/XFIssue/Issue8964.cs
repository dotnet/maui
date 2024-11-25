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

	//[Test]
	//[Category(UITestCategories.CarouselView)]
	//	[FailsOnAndroid]
	//	[FailsOnIOS]
	//	public void Issue8964Test()
	//	{
	//		App.WaitForElement(q => q.Marked($"Item Position - 4"));
	//		var rect = App.Query("carouseView")[0].Rect;
	//		SwipePreviousItem(rect);
	//		App.WaitForElement(q => q.Marked($"Item Position - 4"));
	//		SwipePreviousItem(rect);
	//		App.WaitForElement(q => q.Marked($"Item Position - 4"));
	//		SwipePreviousItem(rect);
	//		App.WaitForElement(q => q.Marked($"Item Position - 4"));
	//		SwipePreviousItem(rect);
	//		App.WaitForElement(q => q.Marked($"Item Position - 4"));
	//		SwipePreviousItem(rect);
	//		App.WaitForElement(q => q.Marked($"Item Position - 4"));
	//		App.WaitForElement(q => q.Marked($"Counter 6"));

	//	}

	//	void SwipePreviousItem(Xamarin.UITest.Queries.AppRect rect)
	//	{
	//#if ANDROID
	//		App.DragCoordinates(rect.X + 10, rect.Y, rect.X + rect.Width - 10, rect.Y);
	//#else
	//		App.SwipeLeftToRight("carouseView");
	//#endif
	//	}
}