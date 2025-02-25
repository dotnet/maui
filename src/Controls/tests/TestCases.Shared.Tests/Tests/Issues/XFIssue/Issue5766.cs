using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue5766 : _IssuesUITest
{
	public Issue5766(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Frame size gets corrupted when ListView is scrolled";

	// TODO: some Xamarin.UITest specific methods that need replacements
	//Xamarin.UITest.Queries.AppRect[] GetLabels(Xamarin.UITest.IApp RunningApp, string label)
	//{
	//	return RunningApp
	//		.Query(q => q.Class("FormsTextView"))
	//		.Where(x => x.Text == label)
	//		.Select(x => x.Rect)
	//		.ToArray();
	//}

	//bool RectIsEquals(Xamarin.UITest.Queries.AppRect[] left, Xamarin.UITest.Queries.AppRect[] right)
	//{
	//	if (left.Length != right.Length)
	//		return false;

	//	for (int i = 0; i < left.Length; i++)
	//	{
	//		if (left[i].X != right[i].X ||
	//			left[i].Y != right[i].Y ||
	//			left[i].Width != right[i].Width ||
	//			left[i].Height != right[i].Height)
	//			return false;
	//	}

	//	return true;
	//}

	//[Test]
	//[Category(UITestCategories.Layout)]
	//[Ignore("Fails sometimes - needs a better test")]
	//public void FrameSizeGetsCorruptedWhenListViewIsScrolled()
	//{
	//	App.WaitForElement(StartText1);
	//	var start = GetLabels(RunningApp, StartText1);
	//	var smalls = GetLabels(RunningApp, SmallText1);
	//	var bigs = GetLabels(RunningApp, BigText1);

	//	App.ScrollDownTo(EndText1, List1, ScrollStrategy.Gesture, 0.9, 15000, timeout: TimeSpan.FromMinutes(1));
	//	App.ScrollUpTo(StartText1, List1, ScrollStrategy.Gesture, 0.9, 15000, timeout: TimeSpan.FromMinutes(1));

	//	var startAfter = GetLabels(RunningApp, StartText1);
	//	Assert.IsTrue(RectIsEquals(start, startAfter));
	//	var smallAfter = GetLabels(RunningApp, SmallText1);
	//	Assert.IsTrue(RectIsEquals(smalls, smallAfter));
	//	var bigAfter = GetLabels(RunningApp, BigText1);
	//	Assert.IsTrue(RectIsEquals(bigs, bigAfter));

	//	// list2 with ListViewCachingStrategy.RecycleElement - issue 6297
	//	App.WaitForElement(StartText2);
	//	start = GetLabels(RunningApp, StartText2);
	//	smalls = GetLabels(RunningApp, SmallText2);
	//	bigs = GetLabels(RunningApp, BigText2);

	//	App.ScrollDownTo(EndText2, List2, ScrollStrategy.Gesture, 0.9, 15000, timeout: TimeSpan.FromMinutes(1));
	//	App.ScrollUpTo(StartText2, List2, ScrollStrategy.Gesture, 0.9, 15000, timeout: TimeSpan.FromMinutes(1));

	//	startAfter = GetLabels(RunningApp, StartText2);
	//	Assert.IsTrue(RectIsEquals(start, startAfter));
	//	smallAfter = GetLabels(RunningApp, SmallText2);
	//	Assert.IsTrue(RectIsEquals(smalls, smallAfter));
	//	bigAfter = GetLabels(RunningApp, BigText2);
	//	Assert.IsTrue(RectIsEquals(bigs, bigAfter));
	//}
}