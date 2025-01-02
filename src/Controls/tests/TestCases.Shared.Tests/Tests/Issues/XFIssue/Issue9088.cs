using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue9088 : _IssuesUITest
{
	public Issue9088(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] SwipeView items conflict with Shell menu swipe in from left, on real iOS devices";

	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void Issue9088SwipeViewConfictWithShellMenuSwipeInFromLeft()
	//{
	//	App.WaitForElement(x => x.Marked(SwipeViewId));

	//	App.SwipeRightToLeft(SwipeViewId);
	//	App.WaitFor(
	//		() => App.Query(x => x.Marked(LeftCountLabelId)).FirstOrDefault()?.Text == "1",
	//		"Swipe left failed at 1. swipe with speed 500");

	//	App.SwipeRightToLeft(SwipeViewId, 0.67, 250);
	//	App.WaitFor(
	//		() => App.Query(x => x.Marked(LeftCountLabelId)).FirstOrDefault()?.Text == "2",
	//		"Swipe left failed at 2. swipe with speed 250");

	//	App.SwipeRightToLeft(SwipeViewId, 0.67, 100);
	//	App.WaitFor(
	//		() => App.Query(x => x.Marked(LeftCountLabelId)).FirstOrDefault()?.Text == "3",
	//		"Swipe left failed at 3. swipe with speed 100");


	//	App.SwipeLeftToRight(SwipeViewId, 0.67, 100);
	//	App.WaitFor(
	//		() => App.Query(x => x.Marked(RightCountLabelId)).FirstOrDefault()?.Text == "1",
	//		"Swipe right failed at 1. swipe with speed 100");

	//	App.SwipeLeftToRight(SwipeViewId, 0.67, 250);
	//	App.WaitFor(
	//		() => App.Query(x => x.Marked(RightCountLabelId)).FirstOrDefault()?.Text == "2",
	//		"Swipe right failed at 2. swipe with speed 250");

	//	App.SwipeLeftToRight(SwipeViewId, 0.67, 500);
	//	App.WaitFor(
	//		() => App.Query(x => x.Marked(RightCountLabelId)).FirstOrDefault()?.Text == "3",
	//		"Swipe right failed at 3. swipe with speed 500");


	//	App.SwipeRightToLeft(SwipeViewId);
	//	App.WaitFor(
	//		() => App.Query(x => x.Marked(LeftCountLabelId)).FirstOrDefault()?.Text == "4",
	//		"Swipe left failed at 4. swipe  with speed 500");

	//	App.SwipeLeftToRight(SwipeViewId);
	//	App.WaitFor(
	//		() => App.Query(x => x.Marked(RightCountLabelId)).FirstOrDefault()?.Text == "4",
	//		"Swipe right failed at 4. swipe with speed 500");

	//	App.SwipeRightToLeft(SwipeViewId);
	//	App.WaitFor(
	//		() => App.Query(x => x.Marked(LeftCountLabelId)).FirstOrDefault()?.Text == "5",
	//		"Swipe left failed at 4. swipe with speed 500");

	//	App.SwipeLeftToRight(SwipeViewId);
	//	App.WaitFor(
	//		() => App.Query(x => x.Marked(RightCountLabelId)).FirstOrDefault()?.Text == "5",
	//		"Swipe right failed at 4. swipe with speed 500");

	//	App.SwipeLeftToRight(SwipeViewId);
	//	App.WaitFor(
	//		() => App.Query(x => x.Marked(RightCountLabelId)).FirstOrDefault()?.Text == "6",
	//		"Swipe right failed at 4. swipe with speed 500");

	//	App.SwipeRightToLeft(SwipeViewId);
	//	App.WaitFor(
	//		() => App.Query(x => x.Marked(LeftCountLabelId)).FirstOrDefault()?.Text == "6",
	//		"Swipe left failed at 4. swipe with speed 500");
	//}
}